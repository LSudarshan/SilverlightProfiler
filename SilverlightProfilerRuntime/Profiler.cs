﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace SilverlightProfilerRuntime
{
    public class Profiler
    {
        private static readonly Stack<Call> stack = new Stack<Call>(new[] {new Call("Root", "", null),});
        private static bool shouldProfile;

        public static void EnteringMethod()
        {
            if (!shouldProfile) return;
            DateTime startTime = DateTime.Now;
            StackFrame frame = new StackTrace().GetFrame(2);
            MethodBase method = frame.GetMethod();
            Call parent = Parent();
            string classWhichOwnsMethod = method.DeclaringType == null ? "" : method.DeclaringType.Name;
            Debug.WriteLine("Entering " + classWhichOwnsMethod + "." + method.Name);
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
            stack.Push(call);
            call.Enter(startTime);
        }

        private static Call Parent()
        {
            return stack.Peek();
        }

        public static void ExitingMethod()
        {
            if (!shouldProfile) return;
            Call call = stack.Pop();
            call.Exit(DateTime.Now);
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
            var window = new ProfilerOutputWindow();
            window.DataContext = stack.Peek();
            window.Show();
        }

        private static void StartProfiling()
        {
            stack.Clear();
            stack.Push(new Call("Root", "", null));
            shouldProfile = true;
            MessageBox.Show("Starting profiling");
        }
    }
}