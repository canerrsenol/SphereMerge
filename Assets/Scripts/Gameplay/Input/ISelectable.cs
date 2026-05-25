// Describes an object that can be selected by player input.
public interface ISelectable
{
    bool CanSelect { get; }
    // Handles a successful selection request.
    void OnSelect();
}
