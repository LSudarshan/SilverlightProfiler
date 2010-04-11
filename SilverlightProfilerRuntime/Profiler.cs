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
        public static readonly Dictionary<int, Stack<Call>> stacksPerThread = new Dictionary<int, Stack<Call>>();
        private static bool shouldProfile;
        public static ProfilerStartWindow StartWindow;
        public static ProfilerOutputWindow ProfilerOutputWindow;

        private static Stack<Call> Stack
        {
            get
            {
                int key = Thread.CurrentThread.GetHashCode();
                if (stacksPerThread.ContainsKey(key))
                    return stacksPerThread[key];
                var newStack = new Stack<Call>();
                newStack.Push(new ThreadRoot(key));
                stacksPerThread[key] = newStack;
                return newStack;
            }
        }

        public static void EnteringMethod(string methodName)
        {
            if (!shouldProfile) return;
            try
            {
//                Debug.WriteLine("Entering " + methodName);
                DateTime startTime = DateTime.Now;
                Call parent = Parent();
                var call = new Call(methodName, parent);
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

        private static Call Parent()
        {
            return Stack.Peek();
        }

        public static void ExitingMethod(string methodName)
        {
            if (!shouldProfile) return;
            try
            {
//                Debug.WriteLine("Exiting " + methodName);
                DateTime time = DateTime.Now;
                ValidateStackAndFixItIfImbalanced(methodName);
                Call call = Stack.Pop();
                call.Exit(time);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
        
        public static void ExceptionThrown()
        {
            try
            {
                if (!shouldProfile) return;
                DateTime time = DateTime.Now;
                MethodBase method = new StackTrace().GetFrame(1).GetMethod();
                string methodName = method.DeclaringType.FullName + "." + method.Name;
                Debug.WriteLine("Exiting via exception " + methodName);
                ValidateStackAndFixItIfImbalanced(methodName);
                Call call = Stack.Pop();
                call.Exit(time);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private static void ValidateStackAndFixItIfImbalanced(string methodName)
        {
            Call currentCall = Stack.Peek();
            while (currentCall.FullName != methodName)
            {
                Stack.Pop();
                if (Stack.Count == 1)
                {
                    string message = string.Format("expected {0} but was {1}", currentCall.FullName, methodName);
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

        public static void StopProfiling()
        {
            shouldProfile = false;
            var threadRoots = new List<Call>(stacksPerThread.Values.Cast<Stack<Call>>().Select(stack => Root(stack)));
            Debug.WriteLine("Number of threads - " + threadRoots.Count);
            var root = new Call("all threads", null);
            root.Children.AddRange(threadRoots);
            ProfilerOutputWindow = new ProfilerOutputWindow(root);
            ProfilerOutputWindow.Show();
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

        public static void StartProfiling()
        {
            stacksPerThread.Clear();
            shouldProfile = true;
            StartWindow = new ProfilerStartWindow();
            StartWindow.Show();
        }
    }
}