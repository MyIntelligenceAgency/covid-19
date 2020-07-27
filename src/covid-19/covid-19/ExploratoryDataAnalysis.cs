﻿using Microsoft.Data.Analysis;
using XPlot.Plotly;
using System;
using System.Collections.Generic;
using System.Linq;

namespace covid_19
{
    public class ExploratoryDataAnalysis
    {
        string newFileName = "05-27-2020_new.csv";

        #region Column Names

        static string COUNTRY = "Country_Region";
        static string LAST_UPDATE = "Last_Update";
        static string CONFIRMED = "Confirmed";
        static string DEATHS = "Deaths";
        static string RECOVERED = "Recovered";
        static string ACTIVE = "Active";

        #endregion

        #region File

        const string DATASET_FILE = "05-27-2020";
        const string FILE_EXTENSION = ".csv";
        const string NEW_FILE_SUFFIX = "_new";
        const char SEPARATOR = ',';
        const char SEPARATOR_REPLACEMENT = '_';
        const string DATASET_GITHUB_DIRECTORY = "https://raw.githubusercontent.com/CSSEGISandData/COVID-19/master/csse_covid_19_data/csse_covid_19_daily_reports/";

        #endregion

        #region DataFrame/Table

        const int TOP_COUNT = 5;
        const int DEFAULT_ROW_COUNT = 10;
        const string VALUES = "Values";
        const string INDIA = "India";

        #endregion

        public void Run()
        {
            #region Load Dataset

            var covid19Dataframe = DataFrame.LoadCsv(newFileName);

            #endregion

            #region Data Range

            var dateRangeDataFrame = covid19Dataframe.Columns[LAST_UPDATE].ValueCounts();
            var dataRange = dateRangeDataFrame.Columns[VALUES].Sort();
            var lastElementIndex = dataRange.Length - 1;

            var startDate = DateTime.Parse(dataRange[0].ToString()).ToShortDateString();
            var endDate = DateTime.Parse(dataRange[lastElementIndex].ToString()).ToShortDateString(); // Last Element
            Console.WriteLine($"The data is between {startDate} and {endDate}");

            #endregion

            #region Display data

            // Default Rows
            var topDefaultRows = covid19Dataframe;

            // Top 5 Rows
            var topRows = covid19Dataframe.Head(5);

            // Random 6 Rows 
            var randomRows = covid19Dataframe.Sample(6);

            // Description
            var description = covid19Dataframe.Description();

            // Information
            var information = covid19Dataframe.Info();

            #endregion

            #region Data Cleaning

            // Active = Confirmed - Deaths - Recovered

            // Filter : Gets active records with negative values
            PrimitiveDataFrameColumn<bool> invalidActiveFilter = covid19Dataframe.Columns[ACTIVE].ElementwiseLessThan(0.0);
            var invalidActiveDataFrame = covid19Dataframe.Filter(invalidActiveFilter);
            Console.WriteLine(invalidActiveDataFrame);

            // Active(-13) = Confirmed(10) - Deaths(51) - Recovered(0)

            // Remove invalid active cases by applying filter
            PrimitiveDataFrameColumn<bool> activeFilter = covid19Dataframe.Columns[ACTIVE].ElementwiseGreaterThanOrEqual(0.0);
            covid19Dataframe = covid19Dataframe.Filter(activeFilter);
            Console.WriteLine(covid19Dataframe.Description());

            #endregion

            #region Visualization

            #region Global

            #region Confirmed Vs Deaths Vs Receovered cases

            //  Gets the collection of confirmed, deaths and recovered
            var confirmed = covid19Dataframe.Columns[CONFIRMED];
            var deaths = covid19Dataframe.Columns[DEATHS];
            var recovered = covid19Dataframe.Columns[RECOVERED];

            // Gets the sum of collection by using Sum method of DataFrame
            var totalConfirmed = Convert.ToDouble(confirmed.Sum());
            var totalDeaths = Convert.ToDouble(deaths.Sum());
            var totaRecovered = Convert.ToDouble(recovered.Sum());

            var confirmedVsDeathsVsRecoveredPlot = Chart.Plot(
                new Graph.Pie()
                {
                    values = new double[] { totalConfirmed, totalDeaths, totaRecovered },
                    labels = new string[] { CONFIRMED, DEATHS, RECOVERED }
                }
            );

            #endregion

            #region Top 5 Countries with Confirmed cases

            // The data for top 5 countries is not present in the csv file.
            // In order to get that, first DataFrame's GROUPBY is used aginst the country.
            // Then it was aggregated using SUM on Confirmed column.
            // In the last, ORDERBYDESCENDING is used to get the top five countries.

            var countryConfirmedGroup = covid19Dataframe.GroupBy(COUNTRY).Sum(CONFIRMED).OrderByDescending(CONFIRMED);
            var topCountriesColumn = countryConfirmedGroup.Columns[COUNTRY];
            var topConfirmedCasesByCountry = countryConfirmedGroup.Columns[CONFIRMED];

            HashSet<string> countries = new HashSet<string>(TOP_COUNT);
            HashSet<long> confirmedCases = new HashSet<long>(TOP_COUNT);
            for (int index = 0; index < TOP_COUNT; index++)
            {
                countries.Add(topCountriesColumn[index].ToString());
                confirmedCases.Add(Convert.ToInt64(topConfirmedCasesByCountry[index]));
            }

            var title = "Top 5 Countries : Confirmed";
            var series1 = new Graph.Bar
            {
                x = countries.ToArray(),
                y = confirmedCases.ToArray()
            };

            var chart = Chart.Plot(new[] { series1 });
            chart.WithTitle(title);
            // display(chart);

            #endregion

            #region Top 5 Countries with Deaths

            // Get the data
            var countryDeathsGroup = covid19Dataframe.GroupBy(COUNTRY).Sum(DEATHS).OrderByDescending(DEATHS);
            topCountriesColumn = countryDeathsGroup.Columns[COUNTRY];
            var topDeathCasesByCountry = countryDeathsGroup.Columns[DEATHS];

            countries = new HashSet<string>(TOP_COUNT);
            HashSet<long> deathCases = new HashSet<long>(TOP_COUNT);
            for (int index = 0; index < TOP_COUNT; index++)
            {
                countries.Add(topCountriesColumn[index].ToString());
                deathCases.Add(Convert.ToInt64(topDeathCasesByCountry[index]));
            }

            #endregion

            #region Top 5 Countries with Recovered cases

            // Get the data
            var countryRecoveredGroup = covid19Dataframe.GroupBy(COUNTRY).Sum(RECOVERED).OrderByDescending(RECOVERED);
            topCountriesColumn = countryRecoveredGroup.Columns[COUNTRY];
            var topRecoveredCasesByCountry = countryRecoveredGroup.Columns[RECOVERED];

            countries = new HashSet<string>(TOP_COUNT);
            HashSet<long> recoveredCases = new HashSet<long>(TOP_COUNT);
            for (int index = 0; index < TOP_COUNT; index++)
            {
                countries.Add(topCountriesColumn[index].ToString());
                recoveredCases.Add(Convert.ToInt64(topRecoveredCasesByCountry[index]));
            }

            title = "Top 5 Countries : Recovered";
            series1 = new Graph.Bar
            {
                x = countries.ToArray(),
                y = recoveredCases.ToArray()
            };

            chart = Chart.Plot(new[] { series1 });
            chart.WithTitle(title);
            // display(chart);

            #endregion

            #endregion

            #region India

            #region Confirmed Vs Deaths Vs Receovered cases

            // Filtering on Country column with INDIA as value

            PrimitiveDataFrameColumn<bool> indiaFilter = covid19Dataframe.Columns[COUNTRY].ElementwiseEquals(INDIA);
            var indiaDataFrame = covid19Dataframe.Filter(indiaFilter);

            var indiaConfirmed = indiaDataFrame.Columns[CONFIRMED];
            var indiaDeaths = indiaDataFrame.Columns[DEATHS];
            var indiaRecovered = indiaDataFrame.Columns[RECOVERED];

            var indiaTotalConfirmed = Convert.ToDouble(indiaConfirmed.Sum());
            var indiaTotalDeaths = Convert.ToDouble(indiaDeaths.Sum());
            var indiaTotalRecovered = Convert.ToDouble(indiaRecovered.Sum());

            #endregion

            #endregion

            #endregion
        }
    }
}
