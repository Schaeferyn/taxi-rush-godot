using Godot;
using System;
using System.Collections.Generic;

public partial class Node3DTimedActions : Node3D
{
	private List<double> _timers;
	private List<Action> _actions;
	private List<int> _actionsToRemove;
	
	private bool _timerRunning;
	private int _numActions;


	public override void _EnterTree()
	{
		base._EnterTree();
		
		_timers = new List<double>();
		_actions = new List<Action>();
		_actionsToRemove = new List<int>();
	}
	
	public override void _Process(double delta)
	{
		if (_timerRunning)
		{
			UpdateTimers(delta);
		}
	}

	void UpdateTimers(double delta)
	{
		//string prnt = "";
		for (int i = 0; i < _numActions; i++)
		{
			_timers[i] -= delta;

			//prnt += " - " + i + ": " + _timers[i];
			if (_timers[i] <= 0)
			{
				_actionsToRemove.Add(i);
				_actions[i]?.Invoke();
			}
		}
		
		//GD.Print(prnt);

		if (_actionsToRemove.Count > 0)
		{
			foreach (int index in _actionsToRemove)
			{
				_timers.RemoveAt(index);
				_actions.RemoveAt(index);
				_numActions = _timers.Count;
			}

			_timerRunning = _numActions > 0;
			_actionsToRemove.Clear();
		}
	}

	protected void BeginTimerEvent(float totalTime, Action callAction)
	{
		_timerRunning = true;
		_timers.Add(totalTime);
		_actions.Add(callAction);
		_numActions = _timers.Count;
	}
}
