public abstract class BaseState
{
    public PlayerContext ctx;
    public PlayerStateMachine sm;

    public BaseState(PlayerContext ctx, PlayerStateMachine sm)
    {
        this.ctx = ctx;
        this.sm = sm;
    }

    public virtual void Enter() { }
    public abstract void Tick(float dt);
    public virtual void Exit() { }
}