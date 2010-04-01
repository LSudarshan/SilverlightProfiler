using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Linq;

namespace SilverlightProfilerRuntime
{
    public class Profiler
    {
        private static Dictionary<int, Stack<Call>> stacksPerThread = new Dictionary<int, Stack<Call>>();
        private static bool shouldProfile;

        public static void EnteringMethod()
        {
            if (!shouldProfile) return;
            try
            {
                DateTime startTime = DateTime.Now;
                StackFrame frame = new StackTrace().GetFrame(1);
                MethodBase method = frame.GetMethod();
                string classWhichOwnsMethod = method.DeclaringType == null ? "" : method.DeclaringType.Name;
//                Debug.WriteLine("Entering:" + classWhichOwnsMethod + "." + method.Name);                
                Call parent = Parent();
                

                var call = new Call(method.Name, classWhichOwnsMethod, parent);
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
            } catch(Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        private static Stack<Call> Stack
        {
            get
            {
                int key = Thread.CurrentThread.GetHashCode();
                if (stacksPerThread.ContainsKey(key))
                    return stacksPerThread[key];
                Stack<Call> newStack = new Stack<Call>();
                newStack.Push(new Call("", Call.THREAD, null));
                stacksPerThread[key] = newStack;
                return newStack;
            }
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
                if (!Stack.Peek().IsThreadRoot)
                {
                    Call call = Stack.Pop();
//                    Debug.WriteLine("Exiting:" + call.FullName);                
                    call.Exit(DateTime.Now);
                }
                else
                {
                    //This is a buggy condition. 
                    Debug.WriteLine("BUG!!! exiting a method when the stacks root is thread node.");
                }
            } catch(Exception e)
            {
                Debug.WriteLine(e);
                throw;
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
            List<Call> threadRoots = new List<Call>(stacksPerThread.Values.Cast<Stack<Call>>().Select(stack => Root(stack)));
            Debug.WriteLine("Number of threads - " + threadRoots.Count);
            Call root = new Call("", "all threads", null);
            root.Children.AddRange(threadRoots);
            var window = new ProfilerOutputWindow(root);
            window.Show();
        }

        private static Call Root(Stack<Call> stack)
        {
            Debug.WriteLine("Stack has " + stack.Count + " Calls");
            while(stack.Count > 1)
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