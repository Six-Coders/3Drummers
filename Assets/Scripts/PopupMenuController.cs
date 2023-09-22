using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;


namespace AlertDialog
{
    public class PopupWindow : VisualElement
    {
        private string tittle = "Error!";
        private string message = "Test...";
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<PopupWindow> { }


        private const string styleResource = "AlertDialogStyle";
        private const string popupClass = "popupWindow";
        private const string popupContainer = "popup_container";
        private const string popupHorizontalContainer = "horizontal_container";
        private const string popupText = "popup_text";
        private const string popupButton = "popup_button";
        private const string popupTypeText = "type_text";
        private const string popupTypeContainer = "type_container";
        private const string popupButtonContainer = "button_container";
        
        private Label typeLabel = new Label();
        private Label messageLabel = new Label();


        public PopupWindow()
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
            AddToClassList(popupContainer);
            VisualElement window = new VisualElement();
            window.AddToClassList(popupClass);
            hierarchy.Add(window);

            //Tittle Selection
            VisualElement typeContainerText = new VisualElement();
            typeContainerText.AddToClassList(popupTypeContainer);
            window.Add(typeContainerText);

            
            typeLabel.text = tittle;
            typeLabel.AddToClassList(popupTypeText);
            typeContainerText.Add(typeLabel);

            //Text Section
            VisualElement horizontalContainerText = new VisualElement();
            horizontalContainerText.AddToClassList(popupHorizontalContainer);
            window.Add(horizontalContainerText);

            
            messageLabel.text = message;
            messageLabel.AddToClassList(popupText);
            horizontalContainerText.Add(messageLabel);

            //Botón
            VisualElement buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(popupButtonContainer);
            window.Add(buttonContainer);
            Button acceptButton = new Button() { text = "Accept" };
            acceptButton.AddToClassList(popupButton);
            buttonContainer.Add(acceptButton);

            acceptButton.clicked += OnAccept;
        }

        public event Action Accept;
        private void OnAccept()
        {
            Accept?.Invoke();
        }
        public void SetTittle(string tittle) 
        {
            typeLabel.text = tittle;
        }
        public void SetMessage(string message) 
        {
            messageLabel.text = message;
        }
    }

}