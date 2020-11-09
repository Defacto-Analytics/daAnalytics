


using daLib.Model;
using daLib.Portfolios;

using daAnalyticsExcel.Exposure;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace daAnalyticsExcel.Ribbon.Forms
{
    public partial class ClearRemoveForm : Form
    {
        private string itemType;
        private string actionType;

        public ClearRemoveForm(string itemType, string actionType)
        {
            InitializeComponent();

            this.itemType = itemType;
            this.actionType = actionType;
        }


        private void ClearRemoveForm_Load(object sender, EventArgs e)
        {
            this.button2.Text = "Exit";

            if (itemType == "model")
            {
                this.label1.Text = "Models in memory:";

                if (actionType == "remove")
                {
                    this.button1.Text = "Remove Selected Model";
                }
                else
                {
                    this.button1.Text = "Clear Selected Model";
                }

            }
            else
            {
                this.label1.Text = "Portfolios in memory:";
                if (actionType == "remove")
                {
                    this.button1.Text = "Remove Selected Portfolio";
                }
                else
                {
                    this.button1.Text = "Clear Selected Portfolio";
                }
            }
            this.updateItems();
        }


        private void updateItems()
        {
            this.listBox1.Items.Clear();
            if(itemType == "model")
            {
                foreach (KeyValuePair<string, CurveModel> kvp in ExcelFunctionExposure.CurveSet)
                {
                    this.listBox1.Items.Add(kvp.Key);
                }
            }
            else
            {
                foreach (KeyValuePair<string, Portfolio> kvp in ExcelFunctionExposure.PortfolioSet)
                {
                    this.listBox1.Items.Add(kvp.Key);
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if(itemType == "model")
            {
                if(actionType == "remove")
                {
                    string CurveModel_ID = this.listBox1.SelectedItem.ToString();

                    // Input validation -- start
                    CurveModel model = ExcelFunctionExposure.TryGetCurveModel(CurveModel_ID);

                    // Input validation -- end
                    ExcelFunctionExposure.CurveSet.Remove(CurveModel_ID.ToLower());
                }
                else
                {
                    string CurveModel_ID = this.listBox1.SelectedItem.ToString();

                    // Input validation -- start
                    CurveModel model = ExcelFunctionExposure.TryGetCurveModel(CurveModel_ID);
                    model.ClearCurveModel();

                    MessageBox.Show(CurveModel_ID + " Cleared");

                }

            }
            else
            {
                if(actionType == "remove")
                {
                    string Portfolio_ID = this.listBox1.SelectedItem.ToString();

                    // Input validation -- start
                    Portfolio port = ExcelFunctionExposure.TryGetPortfolioSet(Portfolio_ID);

                    // Input validation -- end
                    ExcelFunctionExposure.PortfolioSet.Remove(Portfolio_ID.ToLower());
                }
                else
                {
                    string Portfolio_ID = this.listBox1.SelectedItem.ToString();

                    // Input validation -- start
                    Portfolio port = ExcelFunctionExposure.TryGetPortfolioSet(Portfolio_ID);
                    port.ClearPortfolio();

                    MessageBox.Show(Portfolio_ID + " Cleared");
                }
            }


            this.updateItems();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}
