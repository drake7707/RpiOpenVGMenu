using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VGMenu.Screens.Menu;
using System.Globalization;

namespace VGMenu.Screens.Details
{
    public class SubtitleMenuItem : MenuItem
    {

        private List<CultureInfo> availableLanguages;
        private int selectedIndex;

        public SubtitleMenuItem(IDetailItem detailItem)
        {
            availableLanguages = detailItem.PlayableFiles.SelectMany(mp => mp.Subtitles).Where(s => s.Language != null).GroupBy(s => s.Language).Select(g => g.Key).ToList();

            if (detailItem.PlayableFiles.SelectMany(mp => mp.Subtitles).Any(s => s.Language == null))
                availableLanguages.Insert(0, null);

            Console.WriteLine("Available subtitles for movie: " + string.Join(", ", availableLanguages.Select(l => l == null ? "Default" : l.NativeName)));

            if (availableLanguages.Count >= 0)
                selectedIndex = 0; // todo default language here?
        }

        public CultureInfo SelectedLanguage
        {
            get
            {
                if (selectedIndex >= 0 && selectedIndex < availableLanguages.Count)
                    return availableLanguages[selectedIndex];
                else
                    return null;
            }
        }

        public bool UseSubtitles { get { return selectedIndex >= 0 && selectedIndex < availableLanguages.Count; } }

        public void Toggle()
        {
            selectedIndex++;
            if (selectedIndex >= availableLanguages.Count)
                selectedIndex = -1;
        }


        public override string Text
        {
            get
            {
                if (selectedIndex >= 0 && selectedIndex < availableLanguages.Count)
                {
                    if (availableLanguages[selectedIndex] == null)
                        return "Default";
                    else
                        return availableLanguages[selectedIndex].NativeName;
                }
                else
                    return "None";
            }
            set
            {
                base.Text = value;
            }
        }
    }
}
