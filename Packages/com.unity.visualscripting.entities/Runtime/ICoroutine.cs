namespace VisualScripting.Entities.Runtime
{
    public interface ICoroutine
    {
        float DeltaTime { get; set; }
        bool MoveNext();
    }
}
