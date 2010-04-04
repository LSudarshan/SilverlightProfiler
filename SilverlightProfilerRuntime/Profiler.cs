using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace SilverlightProfilerRuntime
{
    public class Profiler
    {
        private static readonly Dictionary<int, Stack<Call>> stacksPerThread = new Dictionary<int, Stack<Call>>();
        private static bool shouldProfile;

        private static Stack<Call> Stack
        {
            get
            {
                int key = Thread.CurrentThread.GetHashCode();
                if (stacksPerThread.ContainsKey(key))
                    return stacksPerThread[key];
                var newStack = new Stack<Call>();
                newStack.Push(new Call("", Call.THREAD, null));
                stacksPerThread[key] = newStack;
                return newStack;
            }
        }

        public static void EnteringMethod()
        {
            if (!shouldProfile) return;
            try
            {
                DateTime startTime = DateTime.Now;
                StackFrame frame = new StackTrace().GetFrame(1);
                MethodBase method = frame.GetMethod();
                Call parent = Parent();
                string className = ClassName(method);
                var call = new Call(method.Name, className, parent);
//                Debug.WriteLine("Entering " + call.FullName);
                if (parent.HasChild(call))
                {
                    call = parent.GetChild(call);
                }
                else
                {
                    parent.Children.Add(call);
                }
                call.IncrementCount();
                Stack.Push(call);
                call.Enter(startTime);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private static string ClassName(MethodBase method)
        {
            return method.DeclaringType == null ? "" : method.DeclaringType.FullName;
        }


        private static Call Parent()
        {
            return Stack.Peek();
        }

        public static void ExitingMethod()
        {
            try
            {
                if (!shouldProfile) return;
                DateTime time = DateTime.Now;
                ValidateStackAndFixItIfImbalanced(new StackTrace().GetFrame(1).GetMethod());
                Call call = Stack.Pop();
                call.Exit(time);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private static void ValidateStackAndFixItIfImbalanced(MethodBase method)
        {
            Call currentCall = Stack.Peek();
            string actualFullName = new Call(method.Name, ClassName(method), null).FullName;
            while (currentCall.FullName != actualFullName)
            {
                Stack.Pop();
                if (Stack.Count == 1)
                {
                    string message = string.Format("expected {0} but was {1}", currentCall.FullName, actualFullName);
                    Debug.WriteLine(message);
                    Debug.WriteLine(new StackTrace());
                    throw new Exception(message);
                }
                currentCall = Stack.Peek();
            }
        }

        public static void Init(UIElement rootVisual)
        {
            rootVisual.KeyDown += (sender, args) => OnKeyDown(args);
        }

        private static void OnKeyDown(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.F8)
            {
                StartProfiling();
            }
            else if (keyEventArgs.Key == Key.F9)
            {
                StopProfiling();
            }
        }

        private static void StopProfiling()
        {
            shouldProfile = false;
            var threadRoots = new List<Call>(stacksPerThread.Values.Cast<Stack<Call>>().Select(stack => Root(stack)));
            Debug.WriteLine("Number of threads - " + threadRoots.Count);
            var root = new Call("", "all threads", null);
            root.Children.AddRange(threadRoots);
            var window = new ProfilerOutputWindow(root);
            window.Show();
        }

        private static Call Root(Stack<Call> stack)
        {
            Debug.WriteLine("Stack has " + stack.Count + " Calls");
            while (stack.Count > 1)
            {
                stack.Pop();
            }
            return stack.Peek();
        }

        private static void StartProfiling()
        {
            stacksPerThread.Clear();
            shouldProfile = true;
            MessageBox.Show("Starting profiling");
        }
    }
}