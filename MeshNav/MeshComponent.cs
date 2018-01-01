namespace MeshNav
{
    public class MeshComponent
    {
        private bool _isAlive = true;
        public bool IsAlive => _isAlive;

        // Once you kill something it's gone forever.  There's no way to "revive" it.
        public void Kill()
        {
            _isAlive = false;
        }
    }
}
