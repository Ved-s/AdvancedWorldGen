using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

//TODO : Add the interface to import settings
namespace AdvancedWorldGen.SeedUI
{
	public class OptionsSelector : UIState
	{
		public static Dictionary<string, Option> OptionDict;
		public LocalizedText Description;

		public UIWorldCreation UiWorldCreation;

		public OptionsSelector(UIWorldCreation uiWorldCreation)
		{
			UiWorldCreation = uiWorldCreation;
			Description = Language.GetText("Mods.AdvancedWorldGen.NoneSelected.description");

			CreateOptionPanel();
		}

		public void GoBack(UIMouseEvent evt, UIElement listeningElement)
		{
			SoundEngine.PlaySound(SoundID.MenuClose);
			Main.MenuUI.SetState(UiWorldCreation);
		}

		public void CreateOptionPanel()
		{
			UIPanel uiPanel = new UIPanel
			{
				HAlign = 0.5f,
				VAlign = 0.5f,
				Width = new StyleDimension(0, 0.5f),
				Height = new StyleDimension(0, 0.5f),
				BackgroundColor = UICommon.MainPanelBackground
			};
			Append(uiPanel);

			UIText uiTitle = new UIText("Seed options", 0.75f, true) {HAlign = 0.5f};
			uiTitle.Height = uiTitle.MinHeight;
			uiPanel.Append(uiTitle);
			uiPanel.Append(new UIHorizontalSeparator
			{
				Width = new StyleDimension(0f, 1f),
				Top = new StyleDimension(43f, 0f),
				Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
			});

			UIText uiDescription = new UIText(Description, 0.75f) {HAlign = 0.5f, VAlign = 0.5f};

			CreateSelectableOptions(uiPanel, uiDescription);

			uiPanel.Append(new UIHorizontalSeparator
			{
				Width = new StyleDimension(0f, 1f),
				Top = new StyleDimension(-50f, 1f),
				Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f
			});

			uiDescription.Height = uiDescription.MinHeight;
			UISlicedImage uIDescriptionBox =
				new UISlicedImage(Main.Assets.Request<Texture2D>("Images/UI/CharCreation/CategoryPanelHighlight"))
				{
					Width = new StyleDimension(0f, 1f),
					Height = new StyleDimension(40f, 0),
					Top = new StyleDimension(-40f, 1f),
					HAlign = 0.5f,
					Color = Color.LightGray * 0.7f
				};
			uIDescriptionBox.SetSliceDepths(10);
			uIDescriptionBox.Append(uiDescription);
			uiPanel.Append(uIDescriptionBox);

			uiPanel.Recalculate();

			UITextPanel<string> goBack = new UITextPanel<string>("Go back")
			{
				Width = new StyleDimension(0f, 0.1f),
				Top = new StyleDimension(0f, 0.75f),
				HAlign = 0.5f
			};
			goBack.OnMouseDown += GoBack;
			goBack.OnMouseOver += UiChanger.FadedMouseOver;
			goBack.OnMouseOut += UiChanger.FadedMouseOut;
			Append(goBack);
		}

		public void CreateSelectableOptions(UIElement uiPanel, UIText uiDescription)
		{
			UIScrollbar uiScrollbar = new UIScrollbar
			{
				Height = new StyleDimension(-110f, 1f),
				Top = new StyleDimension(50, 0f),
				HAlign = 1f
			};
			UIList uiList = new UIList
			{
				Height = new StyleDimension(-110f, 1f),
				Width = new StyleDimension(-20f, 1f),
				Top = new StyleDimension(50, 0f)
			};
			uiList.SetScrollbar(uiScrollbar);
			uiPanel.Append(uiScrollbar);
			uiPanel.Append(uiList);
			CreateOptionList(uiDescription, uiList, false);

			bool showHidden = false;
			LocalizedText showHiddenDescription = Language.GetText("Mods.AdvancedWorldGen.ShowHidden.description");
			LocalizedText hideHiddenDescription = Language.GetText("Mods.AdvancedWorldGen.HideHidden.description");
			UIImage uiImage = new UIImage(TextureAssets.InventoryTickOff)
			{
				HAlign = 1f
			};
			uiImage.OnMouseDown += delegate
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				showHidden = !showHidden;
				uiImage.SetImage(showHidden ? TextureAssets.InventoryTickOn : TextureAssets.InventoryTickOff);
				CreateOptionList(uiDescription, uiList, showHidden);
				uiDescription.SetText(showHidden ? hideHiddenDescription : showHiddenDescription);
			};
			uiImage.OnMouseOver += delegate
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				uiDescription.SetText(showHidden ? hideHiddenDescription : showHiddenDescription);
			};
			uiImage.OnMouseOut += delegate
			{
				SoundEngine.PlaySound(SoundID.MenuTick);
				uiDescription.SetText(Description);
			};
			uiPanel.Append(uiImage);
		}

		public void CreateOptionList(UIText uiDescription, UIList uiList, bool showHidden)
		{
			float currentHeight = 50;
			uiList.Clear();
			bool isLookingAtConflict = false;

			GroupOptionButton<bool> importButton = new GroupOptionButton<bool>(true,
				Language.GetText("Mods.AdvancedWorldGen.Import"),
				Language.GetText("Mods.AdvancedWorldGen.Import.description"), Color.White, null)
			{
				HAlign = 0.5f,
				Width = new StyleDimension(0f, 1f),
				Height = new StyleDimension(40f, 0f),
				Top = new StyleDimension(currentHeight, 0f)
			};
			currentHeight += 40;
			uiList.Add(importButton);

			importButton.SetCurrentOption(false);
			importButton.OnMouseDown += delegate
			{
				HashSet<string> options = TextToOptions(Platform.Get<IClipboard>().Value);
				if (options != null)
				{
					SoundEngine.PlaySound(SoundID.MenuOpen);
					ModifiedWorld.OptionHelper.Options = options;
					CreateOptionList(uiDescription, uiList, showHidden);
				}
			};
			importButton.OnMouseOver += delegate { uiDescription.SetText(importButton.Description); };
			importButton.OnMouseOut += delegate { uiDescription.SetText(Description); };

			foreach (KeyValuePair<string, Option> keyValuePair in OptionDict)
			{
				if (keyValuePair.Value.Hidden && !showHidden) continue;
				string option = keyValuePair.Key;
				GroupOptionButton<bool> clickableText = new GroupOptionButton<bool>(true,
					Language.GetText("Mods.AdvancedWorldGen." + option),
					Language.GetText("Mods.AdvancedWorldGen." + option + ".description"), Color.White, null)
				{
					HAlign = 0.5f,
					Width = new StyleDimension(0f, 1f),
					Height = new StyleDimension(40f, 0f),
					Top = new StyleDimension(currentHeight, 0f)
				};
				currentHeight += 40;
				uiList.Add(clickableText);

				clickableText.SetCurrentOption(ModifiedWorld.OptionHelper.OptionsContains(option));
				clickableText.OnMouseDown += delegate
				{
					bool selected = clickableText.IsSelected;
					if (selected)
						ModifiedWorld.OptionHelper.Options.Remove(option);
					else
						ModifiedWorld.OptionHelper.Options.Add(option);

					if (OptionDict[option].Conflicts
						.Any(conflict => ModifiedWorld.OptionHelper.OptionsContains(conflict)))
						CreateOptionList(uiDescription, uiList, showHidden);
					else
						clickableText.SetCurrentOption(!selected);
				};
				clickableText.OnMouseOver += delegate
				{
					if (!isLookingAtConflict)
						uiDescription.SetText(clickableText.Description);
				};
				clickableText.OnMouseOut += delegate { uiDescription.SetText(Description); };
				if (ModifiedWorld.OptionHelper.OptionsContains(option))
					foreach (string conflict in OptionDict[option].Conflicts)
						if (ModifiedWorld.OptionHelper.OptionsContains(conflict))
						{
							LocalizedText conflictDescription =
								Language.GetText("Mods.AdvancedWorldGen.conflict." + option + "." + conflict);
							UIImage uiImage = new UIImage(UICommon.ButtonErrorTexture)
							{
								Left = new StyleDimension(-15, 0f),
								HAlign = 1f,
								VAlign = 0.5f
							};
							uiImage.OnMouseOver += delegate
							{
								SoundEngine.PlaySound(SoundID.MenuTick);
								uiDescription.SetText(conflictDescription);
								isLookingAtConflict = true;
							};
							uiImage.OnMouseOut += delegate
							{
								SoundEngine.PlaySound(SoundID.MenuTick);
								uiDescription.SetText(clickableText.Description);
								isLookingAtConflict = false;
							};
							clickableText.Append(uiImage);
							break;
						}
			}
		}

		public static HashSet<string> TextToOptions(string text)
		{
			HashSet<string> options = new HashSet<string>();
			foreach (string s in text.Split('|'))
			{
				if (OptionDict.Keys.Contains(s))
				{
					options.Add(s);
				}
				else
				{
					return null;
				}
			}

			return options;
		}
	}
}