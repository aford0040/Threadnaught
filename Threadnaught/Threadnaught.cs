using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ThreadnaughtTaskHandler
{
    public class Threadnaught
    {

        #region Properties
        /// <summary>
        /// The queue of tasks
        /// </summary>
        Dictionary<int, Task> TaskQueue { get; set; } = new Dictionary<int, Task>();

        /// <summary>
        /// Indicates to execute the task when it's added
        /// </summary>
        bool ExecuteOnAdd { get; } = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that sets execute on add.
        /// </summary>
        /// <param name="executeOnAdd">Indicates to execute when a task is added. Default true</param>
        public Threadnaught(bool executeOnAdd)
        {
            ExecuteOnAdd = executeOnAdd;
        }

        /// <summary>
        /// Blank constructor
        /// </summary>
        public Threadnaught()
        {

        }
        #endregion

        #region Public Methods

        #region Add Tasks
        /// <summary>
        /// Adds a new task to the list of tasks and begins execution if beginning the task on add
        /// </summary>
        /// <param name="newTask">The new task to add to the list</param>
        /// <returns>The key of the task within the dictionary</returns>
        public int AddTask(Task newTask)
        {
            // add the task to the queue
            if (TaskQueue.Count <= 0)
                TaskQueue.Add(0, newTask);
            else
            {
                // figure out what the next best key is
                int newKey = 1;
                while (TaskQueue.Keys.Contains(newKey))
                    newKey++;

                // we now have a valid key, add it to dictionary
                TaskQueue.Add(newKey, newTask);
            }

            // Should we start this task?
            if (ExecuteOnAdd)
                ExecuteTask(TaskQueue.Last().Key);

            // return the last key
            return TaskQueue.Last().Key;
        }
        #endregion

        #region Start Task
        /// <summary>
        /// Starts a task with the given key
        /// </summary>
        /// <param name="key">The key to begin</param>
        public void StartTask(int key) =>
            ExecuteTask(key);
        #endregion

        #region Task Gets

        /// <summary>
        /// Returns a given task
        /// </summary>
        /// <param name="key">the key of the task to get</param>
        /// <returns></returns>
        public Task GetTask(int key) =>
            TaskQueue[key];

        /// <summary>
        /// retrieves a value from a task with the given key
        /// </summary>
        /// <typeparam name="T">the type we're expecting to return</typeparam>
        /// <param name="key">The key of the task to get</param>
        public T GetTaskValue<T>(int key)
        {
            // wait for the task then return it's result
            WaitForTask(key);
            return ((Task<T>)GetTask(key)).Result;
        }

        #endregion


        #region Task Waiting

        /// <summary>
        /// Waits for a task with a specific key
        /// </summary>
        /// <param name="key">They key of the needed task</param>
        public void WaitForTask(int key)
        {
            // make sure the task is running
            if (TaskQueue[key].Status == TaskStatus.Created)
                TaskQueue[key].Start();

            // wait for it......
            TaskQueue[key].Wait();
        }
        /// <summary>
        /// Waits for all tasks to complete with the given key
        /// </summary>
        /// <param name="keys">The keys of the tasks to wait for</param>
        public void WaitForTasks(IEnumerable<int> keys)
        {
            
            // run our own home brewed wait for task because if the task hasnt started and you sit
            // in a Task.WaitAll(), you'll sit there forever. Bad news
            keys.ToList().ForEach(m => WaitForTask(m));
        }
        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Executes the task with the given key
        /// </summary>
        /// <param name="key">the key of the task to execute</param>
        private void ExecuteTask(int key) =>
            TaskQueue[key].Start();
        #endregion
    }
}
