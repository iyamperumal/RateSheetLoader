using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using CcshlRateSheet;

namespace OleDbLoader
{
    public class Utilities
    {
        CcshlEntities _ccshlEntities;
        private int LenderId = Convert.ToInt32(ConfigurationManager.AppSettings.Get("LenderId"));

        public Utilities()
        {
            _ccshlEntities = new CcshlEntities();
            AutoMapperConfig.MapModels();
        }

        public void PopulateSheetsOfExcelFileOleDB(string excelFilePath)
        {
            String connString = string.Empty;
            List<string> excelSheetsList = new List<string>();
            DataSet ds = new DataSet();

            try
            {
                connString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties='Excel 12.0 Xml;HDR=YES;IMEX=1;'", excelFilePath);
                using (OleDbConnection objConn = new OleDbConnection(connString))
                {
                    objConn.Open();
                    OleDbCommand objCmd = new OleDbCommand(@"SELECT * FROM [Sheet1$]", objConn);
                    OleDbDataAdapter oleAdapter = new OleDbDataAdapter();
                    oleAdapter.SelectCommand = objCmd;
                    oleAdapter.Fill(ds);

                    //Clear temp tables
                    _ccshlEntities.RatesTemps.Clear();

                    //Load rates from excel dataset to temp rate table
                    var tempRates = FillPlanRateTables(ds);

                    //Load rates from temp table to live rate table
                    LoadRatesFromTempToLiveTable(tempRates);
                }
            }
            catch (Exception exA1)
            {
                throw exA1;
            }
        }

        #region OLEDB

        private IList<RatesTemp> FillPlanRateTables(DataSet ds)
        {

            var planLocations = FindPlanLocations(ds);
            var workSheetTable = ds.Tables[0];
            var planDataSet = new DataSet();
            List<RatesTemp> tempRates = new List<RatesTemp>();

            foreach (KeyValuePair<int, string> planLocation in planLocations)
            {
                int planRowIndex = 0,
                    planColIndex = 0,
                    rateStartRowIndex = 0,
                    planStartColIndex = 0;
                string[] location = planLocation.Value.Split('_');
                PlansTemp plan = _ccshlEntities.PlansTemps.First(p => p.Planid == planLocation.Key);
                string userName = Environment.UserName;
                DateTime currentDate = DateTime.Now;

                planRowIndex = Convert.ToInt32(location[0]);
                planColIndex = Convert.ToInt32(location[1]);

                rateStartRowIndex = planRowIndex + 2;
                planStartColIndex = planColIndex;

                for (int r = rateStartRowIndex; r < rateStartRowIndex + 100; r++) //100 is just an assumption
                {
                    RatesTemp tempRate = new RatesTemp();
                    tempRate.PlanId = plan.Planid;
                    tempRate.CreatedBy = userName;
                    tempRate.CreatedDate = currentDate;

                    for (int c = planStartColIndex; c < planStartColIndex + plan.NumberofRateTypes; c++)
                    {
                        if (IsValidNumericCell(workSheetTable.Rows[r][c]))
                        {
                            RateTblValueToRateObject(
                                tempRate,
                                c - planStartColIndex + 1,
                                decimal.Parse(workSheetTable.Rows[r][c].ToString(), NumberStyles.AllowParentheses |
                                                                                               NumberStyles.AllowThousands |
                                                                                               NumberStyles.AllowDecimalPoint));

                        }
                        else
                        {
                            goto Skip;
                        }

                    }
                    tempRates.Add(tempRate);
                }

            Skip:;
            }

            _ccshlEntities.RatesTemps.AddRange(tempRates);
            _ccshlEntities.SaveChanges();

            return tempRates;
        }

        private void LoadRatesFromTempToLiveTable(IList<RatesTemp> tempRates)
        {
            //Load Temp table to live rates table
            string userName = Environment.UserName;
            DateTime currentDate = DateTime.Now;

            if (tempRates != null &&
                tempRates.Any() &&
                tempRates.Count == _ccshlEntities.RatesTemps.Count())
            {
                var rateHistories =
                   _ccshlEntities.Set<Rate>().ToList()
                   .Select(r =>
                       new RateHistory
                       {
                           PlanId = r.PlanId,
                           RateId = r.RateId,
                           Rate = r.Rate1,
                           Day15 = r.Day15,
                           Day30 = r.Day30,
                           Day45 = r.Day45,
                           Day60 = r.Day60,
                           CreatedBy = r.CreatedBy,
                           CreatedDate = r.CreatedDate,
                           HistoryDate = currentDate
                       }
                   ).ToList();

                //Load data from Rates to RateHistory
                _ccshlEntities.RateHistories
                    .AddRange(rateHistories);
                _ccshlEntities.SaveChanges();

                //Clear data from Rates table
                _ccshlEntities.Rates.Clear();
                _ccshlEntities.SaveChanges();

                //Load data to rates table
                currentDate = DateTime.Now;
                _ccshlEntities.Rates
                    .AddRange(
                        tempRates.Select(r =>
                            new Rate
                            {
                                PlanId = r.PlanId,
                                Rate1 = r.Rate,
                                Day15 = r.Day15,
                                Day30 = r.Day30,
                                Day45 = r.Day45,
                                Day60 = r.Day60,
                                CreatedBy = userName,
                                CreatedDate = currentDate,
                                LastModifiedBy = userName,
                                LastModifiedDate = currentDate
                            }
                        )
                    );

                _ccshlEntities.SaveChanges();
            }
            else
            {
                throw new ApplicationException("RatesTemp records count is not correct.");
            }

        }

        private IDictionary<int, string> FindPlanLocations(DataSet ds)
        {
            int rowIndex = 0, colIndex = 0;
            var planLocations = new Dictionary<int, string>();
            var lenderPlanNames = _ccshlEntities.LenderPlanNamesTemps
                                                .Where(l => l.LenderId == LenderId)
                                                .Select(lpn => new
                                                {
                                                    lpn.PlanId,
                                                    lpn.OtherName
                                                });
            var workSheetTable = ds.Tables[0];

            //Find plan's location
            foreach (DataRow dr in workSheetTable.Rows)
            {
                foreach (DataColumn dc in workSheetTable.Columns)
                {
                    //Check if the cell value is plan name
                    var cellVal = dr[dc];
                    if (cellVal != DBNull.Value &&
                        cellVal != null &&
                        !string.IsNullOrWhiteSpace(cellVal.ToString()) &&
                        lenderPlanNames.Any(lpn => lpn.OtherName == cellVal.ToString().Trim()))
                    {
                        var matchedLenderPlanNames = lenderPlanNames.Where(p => p.OtherName == cellVal.ToString().Trim());

                        matchedLenderPlanNames.ToList().ForEach(p =>
                        {
                            planLocations.Add(p.PlanId, rowIndex + "_" + colIndex);
                        });
                    }

                    colIndex++;
                }
                colIndex = 0;
                rowIndex++;
            }

            return planLocations;
        }

        private bool IsValidNumericCell(object cellVal)
        {
            if (cellVal == DBNull.Value ||
                cellVal == null ||
                string.IsNullOrWhiteSpace(cellVal.ToString().Trim()) ||
                !IsNumber(cellVal))
            {
                return false;
            }

            return true;
        }

        private string RateTblValueToRateObject(RatesTemp rate, int colIndex, decimal rateValue)
        {
            string colName = string.Empty;
            switch (colIndex)
            {
                case 1:
                    rate.Rate = rateValue;
                    break;
                case 2:
                    rate.Day15 = rateValue;
                    break;
                case 3:
                    rate.Day30 = rateValue;
                    break;
                case 4:
                    rate.Day45 = rateValue;
                    break;
                case 5:
                    rate.Day60 = rateValue;
                    break;
            }
            return colName;
        }

        private bool IsNumber(object value)
        {
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(value), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }
        #endregion
    }
}
