using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace InfPopup 
{
    public class InfoPopup : VisualElement
    {
        [UnityEngine.Scripting.Preserve]
        public new class UxmlFactory : UxmlFactory<InfoPopup> { }

        //Load Elements
        private const string styleResources = "infoPopupStyle";
        private const string containerClass = "window-Container";
        private const string windowClass = "popup-Window";
        private const string titleClass = "title-Container";
        private const string titleTextClass = "title-Text-Class";
        private const string infoContainerClass = "info-Container";
        private const string leftRowContainerClass = "left-row";
        private const string rectangleClass = "rectangle";
        private const string normalTextClass = "normal-Text-Class";
        private const string centerRowClass = "center-row";
        private const string tableContainerClass = "table-Container";

        private string titleString = "Information";
        public InfoPopup() 
        {
            styleSheets.Add(Resources.Load<StyleSheet>(styleResources));
            AddToClassList(containerClass);

            VisualElement window = new VisualElement();
            window.AddToClassList(windowClass);
            hierarchy.Add(window);

            VisualElement titleContainer = new VisualElement();
            titleContainer.AddToClassList(titleClass);
            window.Add(titleContainer);

            Label titleLabel = new Label() {text = titleString };
            titleLabel.AddToClassList(titleTextClass);
            titleContainer.Add(titleLabel);

            VisualElement infoContainer = new VisualElement();
            infoContainer.AddToClassList(infoContainerClass);
            window.Add(infoContainer);

            VisualElement leftRowContainer = new VisualElement();
            leftRowContainer.AddToClassList(leftRowContainerClass);
            infoContainer.Add(leftRowContainer);

            VisualElement symbologyContainer = new VisualElement();
            symbologyContainer.AddToClassList(rectangleClass);
            leftRowContainer.Add(symbologyContainer);

            Label symbologyText = new Label() { text = "Symbology" };
            symbologyText.AddToClassList(normalTextClass);
            symbologyContainer.Add(symbologyText);

            VisualElement intensityContainer = new VisualElement();
            intensityContainer.AddToClassList(rectangleClass);
            leftRowContainer.Add(intensityContainer);

            Label intensityText = new Label() { text = "Intensity" };
            intensityText.AddToClassList(normalTextClass);
            intensityContainer.Add(intensityText);

            VisualElement centralRowContainer = new VisualElement();
            centralRowContainer.AddToClassList(centerRowClass);
            infoContainer.Add(centralRowContainer);

            VisualElement cymbalsContainer = new VisualElement();
            cymbalsContainer.style.flexDirection = FlexDirection.Row;
            cymbalsContainer.style.flexShrink = 1;
            centralRowContainer.Add(cymbalsContainer);

            VisualElement cymbalsContainerLeft = new VisualElement();
            cymbalsContainerLeft.AddToClassList(tableContainerClass);
            cymbalsContainer.Add(cymbalsContainerLeft);
            cymbalsContainerLeft.style.color = Color.white;

            Label cymbalsText = new Label() { text = "Cymbals" };
            cymbalsText.AddToClassList(normalTextClass);
            cymbalsContainerLeft.Add(cymbalsText);

            VisualElement cymbalsImage = new VisualElement();
            cymbalsImage.AddToClassList(tableContainerClass);
            cymbalsImage.style.backgroundImage = Resources.Load<Texture2D>("cymbal");

            cymbalsContainer.Add(cymbalsImage);

            VisualElement cymbalsNoteIcon = new VisualElement();
            cymbalsNoteIcon.AddToClassList(tableContainerClass);
            cymbalsNoteIcon.style.backgroundImage = Resources.Load<Texture2D>("glow");
            cymbalsContainer.Add(cymbalsNoteIcon);
        }
    }
}

