using UnityEngine;
using UnityEngine.UIElements;

namespace AlertDialog 
{
    public class Popup_Setup : MonoBehaviour
    {
        public void CreateDialog(string tittle = "",string message="") 
        {
            UIDocument ui = GetComponent<UIDocument>();
            VisualElement root = ui.rootVisualElement;
            PopupWindow popupWindow = new PopupWindow();
            popupWindow.SetTittle(tittle);
            popupWindow.SetMessage(message);
            root.Add(popupWindow);
            popupWindow.Accept += () => root.Remove(popupWindow);
        }
    }
}
