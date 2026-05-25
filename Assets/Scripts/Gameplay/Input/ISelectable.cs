public interface ISelectable
{
    bool CanSelect { get; }
    void OnSelect();
}
