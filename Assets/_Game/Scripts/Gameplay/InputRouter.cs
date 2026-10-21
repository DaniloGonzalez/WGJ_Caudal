using UnityEngine;
using UnityEngine.InputSystem;
namespace Caudal
{
    [System.Serializable]
    public class RoleKeys
    {
        public Key up, down, left, right, confirm, cancel, alternate;
        public RoleKeys(Key u, Key d, Key l, Key r, Key c, Key x, Key a)
        {
            up = u;
            down = d;
            left = l;
            right = r;
            confirm = c;
            cancel = x;
            alternate = a;
        }
    }
    public sealed class InputRouter : MonoBehaviour
    {
        public RoleKeys worker = new RoleKeys(Key.UpArrow, Key.DownArrow, Key.LeftArrow, Key.RightArrow, Key.Enter, Key.None, Key.None);
        public RoleKeys boss = new RoleKeys(Key.UpArrow, Key.DownArrow, Key.LeftArrow, Key.RightArrow, Key.Enter, Key.None, Key.None);
        public bool Pressed(Key key) => Keyboard.current != null && key != Key.None && Keyboard.current[key].wasPressedThisFrame;
    }
}
