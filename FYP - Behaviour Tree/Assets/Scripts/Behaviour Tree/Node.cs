using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public enum BTStatus
{
    RUNNING,
    SUCCESS,
    FAILURE
}

public abstract class BTNode
{
    protected Blackboard bb;
    public BTNode(Blackboard bb)
    {
        this.bb = bb;
    }

    public abstract BTStatus Execute();

    public virtual void Reset()
    {
    }
}

public abstract class CompositeNode : BTNode
{
    protected int currentChildIndex = 0;
    protected List<BTNode> children;

    public CompositeNode(Blackboard bb) : base(bb)
    {
        children = new List<BTNode>();
    }

    public void AddChild(BTNode child)
    {
        children.Add(child);
    }

    public override void Reset()
    {
        currentChildIndex = 0;

        // Reset every child
        for (int i = 0; i < children.Count; i++)
        {
            children[i].Reset();
        }
    }
}

public class Selector : CompositeNode
{
    public Selector(Blackboard bb) : base(bb)
    {
    }

    public override BTStatus Execute()
    {
        BTStatus currentStatus = BTStatus.FAILURE;
        
        for (int i = currentChildIndex; i < children.Count; i++)
        {
            if (children[i].Execute() == BTStatus.RUNNING)
            {
                currentStatus = BTStatus.RUNNING;
                currentChildIndex = i;
                return currentStatus;
            }
            else if (children[i].Execute() == BTStatus.SUCCESS)
            {
                currentStatus = BTStatus.SUCCESS;
                Reset();
                return currentStatus;
            }
        }

        Reset();
        return currentStatus;
    }
}

public class Sequence : CompositeNode
{
    public Sequence(Blackboard bb) : base(bb)
    {
    }

    public override BTStatus Execute()
    {
        BTStatus currentStatus = BTStatus.SUCCESS;

        for (int i = currentChildIndex; i < children.Count; i++)
        {
            if (children[i].Execute() == BTStatus.RUNNING)
            {
                currentStatus = BTStatus.RUNNING;
                currentChildIndex = i;
                return currentStatus;
            }
            else if (children[i].Execute() == BTStatus.FAILURE)
            {
                currentStatus = BTStatus.FAILURE;
                Reset();
                return currentStatus;
            }
        }

        Reset();
        return currentStatus;
    }
}

public abstract class DecoratorNode : BTNode
{
    protected BTNode wrappedNode;
    public DecoratorNode(BTNode wrappedNode, Blackboard bb) : base (bb)
    {
        this.wrappedNode = wrappedNode;
    }

    public BTNode GetWrappedNode()
    {
        return wrappedNode;
    }

    public override void Reset()
    {
        wrappedNode.Reset();
    }
}

public class InverterDecorator : DecoratorNode
{
    public InverterDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
    }

    public override BTStatus Execute()
    {
        BTStatus currentStatus = wrappedNode.Execute();

        if (currentStatus == BTStatus.FAILURE)
        {
            currentStatus = BTStatus.SUCCESS;
        }
        else if (currentStatus == BTStatus.SUCCESS)
        {
            currentStatus = BTStatus.FAILURE;
        }

        return currentStatus;
    }
}

public abstract class ConditionalDecorator : DecoratorNode
{
    public ConditionalDecorator(BTNode wrappedNode, Blackboard bb) : base(wrappedNode, bb)
    {
    }

    public abstract bool CheckStatus();
    public override BTStatus Execute()
    {
        BTStatus currentStatus = BTStatus.FAILURE;

        if (CheckStatus())
        {
            currentStatus = wrappedNode.Execute();
        }

        return currentStatus;
    }
}

public class DelayNode : BTNode
{
    protected float delay = 0.0f;
    bool started = false;
    private Timer regulator;
    bool delayFinished = false;

    public DelayNode(Blackboard bb, float delayTime) : base(bb)
    {
        this.delay = delayTime;
        regulator = new Timer(delay * 1000.0f); // in milliseconds so * by 1000
        regulator.Elapsed += OnTimedEvent;
        regulator.Enabled = true;
        regulator.Stop();
    }

    public override BTStatus Execute()
    {
        BTStatus currentStatus = BTStatus.RUNNING;

        if (!started && !delayFinished)
        {
            started = true;
            regulator.Start();
        }
        else if (delayFinished)
        {
            delayFinished = false;
            started = false;
            currentStatus = BTStatus.SUCCESS;
        }

        return currentStatus;
    }

    private void OnTimedEvent(object sender, ElapsedEventArgs e)
    {
        started = false;
        delayFinished = true;
        regulator.Stop();
    }

    public override void Reset()
    {
        regulator.Stop();
        delayFinished = false;
        started = false;
    }
}