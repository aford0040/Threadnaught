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
        public Dictionary<int, Task> TaskQueue { get; } = new Dictionary<int, Task>();

        /// <summary>
        /// Indicates to execute the task when it's added
        /// </summary>
        bool ExecuteOnAdd { get; } = true;

        /// <summary>
        /// The maximum number of tasks threadnaught is allowed to run at one time.
        /// Wouldn't want to overload the potato computers would we?
        /// </summary>
        int MaxRunningTasks { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that sets execute on add.
        /// </summary>
        /// <param name="executeOnAdd">Indicates to execute when a task is added. Default true</param>
        /// <param name="maxRunningTasks">Indicates the maximum number of tasks to run at one time</param>
        public Threadnaught(int maxRunningTasks)
        {
            ExecuteOnAdd = false;
            MaxRunningTasks = maxRunningTasks;
        }

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
        /// <param name="removeOnComplete">Indicates to remove the task when it's done</param>
        /// <returns>The key of the task within the dictionary</returns>
        public int AddTask(Task newTask, bool removeOnComplete = false)
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
                if (removeOnComplete)
                    newTask.ContinueWith(m => RemoveTask(newKey));
                   
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
            // setup the return value
            T returnValue;

            // wait for the task then return it's result
            WaitForTask(key);

            // assign result
            returnValue = ((Task<T>)GetTask(key)).Result;

            // remove task since it isnt needed anymore
            RemoveTask(key);

            // return the value
            return returnValue;
        }

        /// <summary>
        /// Gets the current status of the given task
        /// </summary>
        /// <param name="key">The key of the task to get</param>
        /// <returns></returns>
        public TaskStatus GetTaskStatus(int key) =>
            TaskQueue[key].Status;
        #endregion


        #region Task Waiting

        /// <summary>
        /// Waits for a task with a specific key
        /// </summary>
        /// <param name="key">They key of the needed task</param>
        /// <param name="removeOnComplete">indicates to remove the task when it's completed</param>
        public void WaitForTask(int key, bool removeOnComplete = false)
        {
            // make sure the task is running
            if (TaskQueue[key].Status == TaskStatus.Created)
                ExecuteTask(key);

            // wait for it......
            TaskQueue[key].Wait();

            if (removeOnComplete)
                TaskQueue.Remove(key);
        }

        /// <summary>
        /// Waits for all tasks to complete with the given key
        /// </summary>
        /// <param name="keys">The keys of the tasks to wait for</param>
        /// <param name="removeOnComplete">Indicates to remove the task when it has completed running</param>
        public void WaitForTasks(IEnumerable<int> keys, bool removeOnComplete = false)
        {
            // run our own home brewed wait for task because if the task hasnt started and you sit
            // in a Task.WaitAll(), you'll sit there forever. Bad news
            if (MaxRunningTasks > 0)
                ThrottledWaitForTasks(keys, removeOnComplete);
            else
                keys.ToList().ForEach(m => WaitForTask(m, removeOnComplete));
        }
        #endregion

        #endregion

        #region Private Methods

        #region Throttling Methods
        /// <summary>
        /// Takes into consideration the max number of concurrent tasks 
        /// </summary>
        /// <param name="keys">The keys to wait for</param>
        /// <param name="removeOnComplete">indicates to remove the tasks when they are done</param>
        private void ThrottledWaitForTasks(IEnumerable<int> keys, bool removeOnComplete)
        {
            // run the tasks in batches
            for (int I = 0; I <= (keys.Count() / MaxRunningTasks); I++)
            {
                // only get the batch that's relavent
                List<int> batch = keys.Skip(I * MaxRunningTasks).Take(MaxRunningTasks).ToList();
                foreach (int key in batch)
                    StartTask(key);

                // wait for the throttled tasks
                WaitForThrottledTasks(batch, removeOnComplete);
            }
        }

        /// <summary>
        /// Wait for each task in the throttled list
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="removeOnComplete"></param>
        private void WaitForThrottledTasks(List<int> batch, bool removeOnComplete) =>
            batch.ForEach(m => WaitForTask(m, removeOnComplete)); 
        #endregion

        /// <summary>
        /// Executes the task with the given key
        /// </summary>
        /// <param name="key">the key of the task to execute</param>
        private void ExecuteTask(int key) =>
            TaskQueue[key].Start();

        /// <summary>
        /// Removes a task with the given key
        /// </summary>
        /// <param name="key"></param>
        private void RemoveTask(int key) =>
            TaskQueue.Remove(key);

        #endregion
    }
}
