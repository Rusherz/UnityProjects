using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobQueue {

	Queue<Job> jobQueue;

	Action<Job> cbJobCreated;

	public JobQueue(){
		jobQueue = new Queue<Job> ();
	}

	public void Enqueue(Job j){
		if (j.jobTime < 0) {
			j.DoWork (0);
			return;
		}

		jobQueue.Enqueue (j);
		if (cbJobCreated != null) {
			cbJobCreated (j);
		}

	}

	public Job Dequeue(){
		if (jobQueue.Count == 0) {
			return null;
		}
		return jobQueue.Dequeue ();
	}

	public void Remove(Job j){
		List<Job> jobs = new List<Job> (jobQueue);
		if (jobs.Contains (j) == false) {
			//Debug.LogError ("Trying to remove a job that isnt in the queue.");

			return;
		}
		jobs.Remove (j);
		jobQueue = new Queue<Job> (jobs);
		jobs = null;
	}

	public void RegisterJobCreationCallBack(Action<Job> cb){
		cbJobCreated += cb;
	}

	public void UnregisterJobCreationCallBack(Action<Job> cb){
		cbJobCreated -= cb;
	}


}