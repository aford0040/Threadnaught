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
        [TestCategory("Add")]
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
        [TestCategory("Auto Execute")]
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
        /// Tests retrieving a value from the threadnaught
        /// </summary>
        [TestMethod]
        [TestCategory("Value Retrieval")]
        public void TestValueRetrieveInt()
        {
            // setup handler and add task
            Threadnaught handler = new Threadnaught();
            int taskNumber = 1;

            int task1 = handler.AddTask(new Task<int>(() => WasteTimeInt(taskNumber)));

            // get the value
            Assert.AreEqual(handler.GetTaskValue<int>(task1), taskNumber);
        }

        /// <summary>
        /// Tests retrieving a value from the threadnaught
        /// </summary>
        [TestMethod]
        [TestCategory("Value Retrieval")]
        public void TestValueRetrieveString()
        {
            // setup handler and add task
            Threadnaught handler = new Threadnaught();
            string taskString = "blah blah blah";

            int task1 = handler.AddTask(new Task<string>(() => WasteTimeString(1, taskString)));

            // get the value
            Assert.AreEqual(handler.GetTaskValue<string>(task1), taskString);

        }

        /// <summary>
        /// Wastes time and writes crap to the console.
        /// </summary>
        /// <param name="tracker">The number to keep track of</param>
        /// <returns>Tracker number</returns>
        public int WasteTimeInt(int tracker)
        {
            TimeWaster(tracker);

            return tracker;
        }

        /// <summary>
        /// Wastes time and writes crap to the console.
        /// </summary>
        /// <param name="tracker">The number to keep track of</param>
        /// <param name="testString">The string to return</param>
        /// <returns>Tracker number</returns>
        public string WasteTimeString(int tracker, string testString)
        {
            TimeWaster(tracker);

            return testString;
        }

        #endregion

        #region Test 4 (Waiting For Tasks)

        /// <summary>
        /// Tests waiting for a task as well as tests the
        /// task starting functionality of WaitForTask IF it hasnt started already
        /// </summary>
        [TestMethod]
        [TestCategory("Wait For Tasks")]
        public void TestWait()
        {
            // create the task handler
            Threadnaught handler = new Threadnaught(false); // false to check the auto start on the wait

            // setup some new tasks
            List<Task> newTasks = new List<Task>()
            {
                new Task(() => TimeWaster(1)),
                new Task(() => TimeWaster(2)),
                new Task(() => TimeWaster(3)),
                new Task(() => TimeWaster(4)),
                new Task(() => TimeWaster(5)),
                new Task(() => TimeWaster(6)),
                new Task(() => TimeWaster(7)),
                new Task(() => TimeWaster(8)),
                new Task(() => TimeWaster(9)),
                new Task(() => TimeWaster(0)),

            };

            // setup a list to get the task keys
            List<int> taskKeys = new List<int>();

            // add the tasks to the handler
            newTasks.ForEach(m => taskKeys.Add(handler.AddTask(m)));

            // pick a task to run
            int taskToRun = PickRandomTaskKey(taskKeys);

            handler.WaitForTask(taskToRun);

            Assert.AreEqual(handler.GetTask(taskToRun).Status, TaskStatus.RanToCompletion);
            taskKeys.Where(m => m != taskToRun).ToList().ForEach(m => Assert.AreEqual(handler.GetTask(m).Status, TaskStatus.Created));
        }

        /// <summary>
        /// Tests waiting for multiple tasks as well as tests the
        /// task starting functionality of wait for task IF it hasnt started already
        /// </summary>
        [TestMethod]
        [TestCategory("Wait For Tasks")]
        public void TestWaitMultiple()
        {
            // create the task handler
            Threadnaught handler = new Threadnaught(false); // false to check the auto start on the wait

            // setup some new tasks
            List<Task> newTasks = new List<Task>()
            {
                new Task(() => TimeWaster(1)),
                new Task(() => TimeWaster(2)),
                new Task(() => TimeWaster(3)),
                new Task(() => TimeWaster(4)),
                new Task(() => TimeWaster(5)),
                new Task(() => TimeWaster(6)),
                new Task(() => TimeWaster(7)),
                new Task(() => TimeWaster(8)),
                new Task(() => TimeWaster(9)),
                new Task(() => TimeWaster(0)),
            };

            // setup a list to get the task keys
            List<int> taskKeys = new List<int>();

            // add the tasks to the handler
            newTasks.ForEach(m => taskKeys.Add(handler.AddTask(m)));

            // get the keys we want to run
            List<int> tasksToRun = PickRandomTaskKeys(taskKeys, 4);

            // wait for the tasks
            handler.WaitForTasks(tasksToRun);

            // make sure the tasks are run to completion
            tasksToRun.ForEach(m => Assert.AreEqual(handler.GetTask(m).Status, TaskStatus.RanToCompletion));

        }
        #endregion

        #region Test 5 (Memory Management)

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
                if (I % 5000000 == 0)
                    Console.WriteLine($"We hit a landmark for {tracker}");
        }

        /// <summary>
        /// Picks a single task key
        /// </summary>
        /// <param name="taskList">The list of tasks to choose from</param>
        /// <returns></returns>
        private int PickRandomTaskKey(List<int> taskKeys) =>
            new Random().Next(0, taskKeys.Count);

        /// <summary>
        /// Picks a random number of task keys
        /// </summary>
        /// <param name="taskList">The list of tasks to choose from</param>
        /// <param name="numberOfKeys">the number of keys to pick</param>
        /// <returns></returns>
        private List<int> PickRandomTaskKeys(List<int> taskList, int numberOfKeys = 4)
        {
            // creat the return value
            List<int> taskKeys = new List<int>();

            // pick the number of keys you wish
            for (int I = 0; I < numberOfKeys; I++)
            {
                // create a random to pick indexes from task list
                Random randoInt = new Random();
                int randomIndex = randoInt.Next(0, taskList.Count);

                // while the tsk keys dont contain the randomly picked key, keep picking random indexes
                while (taskKeys.Contains(randomIndex))
                    randomIndex = randoInt.Next(0, taskList.Count);

                // add it
                taskKeys.Add(randomIndex);
            }

            // of course....return
            return taskKeys;

        }
        #endregion
    }
}
