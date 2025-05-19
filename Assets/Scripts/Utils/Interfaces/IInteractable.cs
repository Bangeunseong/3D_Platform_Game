namespace Utils.Interfaces
{
    public interface IInteractable
    {
        public string GetInteractPrompt();
        public void OnInteract();
    }
}