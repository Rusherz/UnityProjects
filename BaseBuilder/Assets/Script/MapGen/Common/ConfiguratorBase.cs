using System.Collections.Generic;
using UnityEngine;

namespace ProceduralToolkit.Examples
{
    public class ConfiguratorBase : MonoBehaviour
    {
        private Palette currentPalette = new Palette();
        private Palette targetPalette = new Palette();

        protected static T InstantiateControl<T>(Transform parent) where T : Component
        {
            T prefab = Resources.Load<T>(typeof (T).Name);
            T control = Instantiate(prefab);
            control.transform.SetParent(parent, false);
            control.transform.localPosition = Vector3.zero;
            control.transform.localRotation = Quaternion.identity;
            control.transform.localScale = Vector3.one;
            return control;
        }

        protected void GeneratePalette()
        {
			//List<Color> palette = RandomE.TetradicPalette(0.25f, 0.7f);
			targetPalette.mainColor = Color.black;
			targetPalette.secondaryColor = Color.black;
			targetPalette.skyColor = Color.black;
			targetPalette.horizonColor = Color.black;
			targetPalette.groundColor = Color.black;
        }

        protected void SetupSkyboxAndPalette()
        {
            RenderSettings.skybox = new Material(RenderSettings.skybox);
            currentPalette.mainColor = targetPalette.mainColor;
            currentPalette.secondaryColor = targetPalette.secondaryColor;
            currentPalette.skyColor = targetPalette.skyColor;
            currentPalette.horizonColor = targetPalette.horizonColor;
            currentPalette.groundColor = targetPalette.groundColor;
        }

        protected void UpdateSkybox()
        {
            LerpSkybox(RenderSettings.skybox, currentPalette, targetPalette, Time.deltaTime);
        }

        private static void LerpSkybox(Material skybox, Palette currentPalette, Palette targetPalette, float t)
        {
			currentPalette.skyColor = Color.Lerp(currentPalette.skyColor, targetPalette.skyColor, t);
			currentPalette.horizonColor = Color.Lerp(currentPalette.horizonColor, targetPalette.horizonColor, t);
			currentPalette.groundColor = Color.Lerp(currentPalette.groundColor, targetPalette.groundColor, t);

            skybox.SetColor("_SkyColor", currentPalette.skyColor);
            skybox.SetColor("_HorizonColor", currentPalette.horizonColor);
            skybox.SetColor("_GroundColor", currentPalette.groundColor);
        }

        private class Palette
        {
			public Color mainColor;
			public Color secondaryColor;
			public Color skyColor;
			public Color horizonColor;
			public Color groundColor;
        }
    }
}