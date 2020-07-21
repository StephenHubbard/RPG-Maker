using RPG.Control;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Movement
{
    public class Door : MonoBehaviour, IRaycastable
    {
        public bool HandleRaycast(PlayerController callingController)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SceneManager.LoadScene("Inn");
            }
            return true;
        }

        public CursorType GetCursorType()
        {
            return CursorType.Door;
        }
    }
}
