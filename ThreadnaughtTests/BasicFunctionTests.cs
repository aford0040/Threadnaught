using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ThreadnaughtTaskHandler;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace ThreadnaughtTests
{
    [TestClass]
    public class BasicFunctionTests
    {
        #region Test 1 (Adding)

        /// <summary>
        /// Tests adding a task to the threadnaught
        /// </summary>
        [TestMethod]
        public void TestAdd()
        {
            Threadnaught handler = new Threadnaught();

            int task1 = handler.AddTask(new Task(() => AddWasteTime(1)));
            int task2 = handler.AddTask(new Task(() => AddWasteTime(2)));
            int task3 = handler.AddTask(new Task(() => AddWasteTime(3)));

            handler.WaitForTask(task1);
            handler.WaitForTask(task2);
            handler.WaitForTask(task3);

            // if any of the stats havent ran to completion, this test failed
            if (new List<TaskStatus>()
            {
                handler.GetTask(task1).Status,
                handler.GetTask(task2).Status,
                handler.GetTask(task3).Status
            }.Any(m => m != TaskStatus.RanToCompletion))
            Assert.Fail();
        }

        /// <summary>
        /// Wastes time and writes crap to the console
        /// </summary>
        /// <param name="tracker">The number to keep track of</param>
        public void AddWasteTime(int tracker) =>
            TimeWaster(tracker);
        #endregion

        #region Test 2 (Non Auto Execute)
        /// <summary>
        /// Tests the auto execute functionality of threadnaught
        /// </summary>
        [TestMethod]
        public void TextAutoExecute()
        {
            Threadnaught handler = new Threadnaught(false);

            int task1 = handler.AddTask(new Task(() => ExecuteWasteTime(1)));
            int task2 = handler.AddTask(new Task(() => ExecuteWasteTime(2)));
            int task3 = handler.AddTask(new Task(() => ExecuteWasteTime(3)));

            handler.StartTask(task1);
            handler.StartTask(task3);

            handler.WaitForTask(task1);
            handler.WaitForTask(task3);

            // if task two rant o completion or ran, it failed
            Task taskTwo = handler.GetTask(task2);
            if (taskTwo.Status == TaskStatus.RanToCompletion ||
                taskTwo.Status == TaskStatus.Running)
                Assert.Fail();
        }

        /// <summary>
        /// Wastes time and writes crap to the console
        /// </summary>
        /// <param name="tracker">The number to keep track of</param>
        public void ExecuteWasteTime(int tracker) =>
            TimeWaster(tracker);
        #endregion

        #region Test 3 (Retrieving Task value)
        /// <summary>
        /// Tests adding a task to the threadnaught
        /// </summary>
        [TestMethod]
        public void TestValueRetrieve()
        {
            // setup handler and add task
            Threadnaught handler = new Threadnaught();

            int task1 = handler.AddTask(new Task<int>(() => WasteTimeValue(1)));

            // get the value
            handler.GetTaskValue<int>(task1);

        }

        /// <summary>
        /// Wastes time and writes crap to the console.
        /// </summary>
        /// <param name="tracker">The number to keep track of</param>
        /// <returns>Tracker number</returns>
        public int WasteTimeValue(int tracker)
        {
            TimeWaster(tracker);

            return tracker;
        }
        #endregion

        #region Multiple Test Used Functions
        /// <summary>
        ///  Used on multiple tests currently
        /// </summary>
        /// <param name="tracker"></param>
        private void TimeWaster(int tracker)
        {
            Console.WriteLine($"Starting time waster using {tracker}");
            for (int I = 0; I < int.MaxValue / 2; I++)
                if (I % 1000 == 0)
                    Console.WriteLine($"We hit a landmark for {tracker}");
        }
        #endregion
    }
}
