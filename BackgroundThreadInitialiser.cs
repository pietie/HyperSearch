using HscLib;
using HyperSearch.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HyperSearch
{
    public class BackgroundThreadInitialiser
    {
        public void Run(object state)
        {
            try
            {
                MainWindow win = (MainWindow)state;

                GameSearchWindow.PerformSearchTask.LoadGenreImageLocations();

                var t = System.Environment.TickCount;
                var count = GameSearchWindow.PerformSearchTask.LoadFullGameList();
                t = System.Environment.TickCount - t;

                win.Log("Search: full game list loaded in {0}ms. Game count: {1}", t, count);

                t = System.Environment.TickCount;
                count = GameSearchWindow.PerformSearchTask.LoadFavourites();
                t = System.Environment.TickCount - t;

                win.Log("Search: Favourites list loaded in {0}ms. Entry count: {1}", t, count);

                t = System.Environment.TickCount;
                count = GameSearchWindow.PerformSearchTask.LoadGenres();
                t = System.Environment.TickCount - t;

                win.Log("Search: Genere list loaded in {0}ms. Genre count: {1}", t, count);
            }
            catch (Exception ex)
            {
                //SessionLogger.Error("Failed to build full search database", ex.Message);
                ErrorHandler.LogException(ex);
            }
        }
    }
}
