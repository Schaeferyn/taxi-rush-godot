using Godot;
using System;
using System.Collections.Generic;

public partial class ControlTimedActions : Control
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
		for (int i = 0; i < _numActions; i++)
		{
			_timers[i] -= delta;

			if (_timers[i] <= 0)
			{
				_actionsToRemove.Add(i);
				_actions[i]?.Invoke();
			}
		}

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
