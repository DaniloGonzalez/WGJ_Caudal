using UnityEngine;
using UnityEngine.InputSystem;
namespace Caudal
{
    [System.Serializable]
    public class RoleKeys
    {
        public Key up, down, confirm, cancel, alternate;
        public RoleKeys(Key u, Key d, Key c, Key x, Key a)
        {
            up = u;
            down = d;
            confirm = c;
            cancel = x;
            alternate = a;
        }
    }
    public sealed class InputRouter : MonoBehaviour
    {
        public RoleKeys worker = new RoleKeys(Key.W, Key.S, Key.F, Key.G, Key.E);
        public RoleKeys boss = new RoleKeys(Key.UpArrow, Key.DownArrow, Key.Enter, Key.RightShift, Key.RightCtrl);
        public bool Pressed(Key key) => Keyboard.current != null && key != Key.None && Keyboard.current[key].wasPressedThisFrame;
    }
}
