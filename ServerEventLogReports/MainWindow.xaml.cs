/* Title:           Server Event Log Reports
 * Date:            10-28-21
 * Author:          Terry Holmes
 * 
 * Description:     This is used to copy from server event log to get it ready for the Server Event Log Reports */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NewEventLogDLL;
using DataValidationDLL;
using DateSearchDLL;

namespace ServerEventLogReports
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        DateSearchClass TheDateSearchClass = new DateSearchClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();

        //setting up the data
        EventLogSercurityDataSet TheEventLogSercurityDataSet = new EventLogSercurityDataSet();
        FindServerEventLogForReportsVerificationDataSet TheFindServerEventLogForReportsVerificationDataSet = new FindServerEventLogForReportsVerificationDataSet();
        FindServerEventLogByDateRangeDataSet TheFindServerEventLogByDateRangeDataSet = new FindServerEventLogByDateRangeDataSet();
        FindServerEventLogForReportsByDateRangeDataSet TheFindServerEventLogForReportsByDateRangeDataSet = new FindServerEventLogForReportsByDateRangeDataSet();

        DateTime gdatStartDate;
        DateTime gdatEndDate;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void expCloseProgram_Expanded(object sender, RoutedEventArgs e)
        {
            expCloseProgram.IsExpanded = false;

            TheMessagesClass.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ResetControls();
        }

        private void btnFind_Click(object sender, RoutedEventArgs e)
        {
            string strValueForValidation;
            string strErrorMessage = "";
            bool blnFatalError = false;
            bool blnThereIsAProblem = false;

            try
            {
                PleaseWait PleaseWait = new PleaseWait();
                PleaseWait.Show();

                strValueForValidation = txtStartDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if(blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The Start Date is not a Date\n";
                }
                else
                {
                    gdatStartDate = Convert.ToDateTime(strValueForValidation);
                }
                strValueForValidation = txtEndDate.Text;
                blnThereIsAProblem = TheDataValidationClass.VerifyDateData(strValueForValidation);
                if (blnThereIsAProblem == true)
                {
                    blnFatalError = true;
                    strErrorMessage += "The End Date is not a Date\n";
                }
                else
                {
                    gdatEndDate = Convert.ToDateTime(strValueForValidation);
                }
                if(blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage(strErrorMessage);
                    return;
                }
                else
                {
                    blnFatalError = TheDataValidationClass.verifyDateRange(gdatStartDate, gdatEndDate);

                    if(blnFatalError == true)
                    {
                        TheMessagesClass.ErrorMessage("The Start Date is after the End Date");
                    }
                }

                TheFindServerEventLogByDateRangeDataSet = TheEventLogClass.FindServerEventLogByDateRange(gdatStartDate, gdatEndDate);

                dgrEvents.ItemsSource = TheFindServerEventLogByDateRangeDataSet.FindServerEventLogByDateRange;

                PleaseWait.Close();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Server Event Log Reports // Main Window // Find Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

        }
        private void ResetControls()
        {
            txtEndDate.Text = "";
            txtStartDate.Text = "";

            TheEventLogSercurityDataSet.eventlogsecurity.Rows.Clear();

            dgrEvents.ItemsSource = TheEventLogSercurityDataSet.eventlogsecurity;
        }

        private void expProcessDay_Expanded(object sender, RoutedEventArgs e)
        {
            
            int intCounter;
            int intNumberOfRecords;
            DateTime datTransactionDate;
            string strLogonName;
            string strItemAccessed;
            string strEventNotes;
            DateTime datStartDate = DateTime.Now;

            try
            {
                expProcessDay.IsExpanded = false;
                TheEventLogSercurityDataSet.eventlogsecurity.Rows.Clear();

                PleaseWait PleaseWait = new PleaseWait();
                PleaseWait.Show();

                intNumberOfRecords = TheFindServerEventLogByDateRangeDataSet.FindServerEventLogByDateRange.Rows.Count;
                

                if (intNumberOfRecords > 0)
                {
                    for (intCounter = 0; intCounter < intNumberOfRecords; intCounter++)
                    {
                        datTransactionDate = TheFindServerEventLogByDateRangeDataSet.FindServerEventLogByDateRange[intCounter].TransactionDate;
                        strLogonName = "Just Beginging";
                        strItemAccessed = "Date Goes Here";
                        strEventNotes = TheFindServerEventLogByDateRangeDataSet.FindServerEventLogByDateRange[intCounter].EventNotes;

                        char[] delims = new[] { '\n', '\t', '\r' };
                        string[] strNewItems = strEventNotes.Split(delims, StringSplitOptions.RemoveEmptyEntries);

                        strLogonName = strNewItems[5];
                        strItemAccessed = strNewItems[16];

                        datTransactionDate = TheDateSearchClass.RemoveTime(datTransactionDate);

                        datTransactionDate = TheDateSearchClass.RemoveTime(datTransactionDate);

                        EventLogSercurityDataSet.eventlogsecurityRow NewEventRow = TheEventLogSercurityDataSet.eventlogsecurity.NeweventlogsecurityRow();

                        NewEventRow.TransactionDate = datTransactionDate;
                        NewEventRow.LogonName = strLogonName;
                        NewEventRow.ItemAccessed = strItemAccessed;

                        TheEventLogSercurityDataSet.eventlogsecurity.Rows.Add(NewEventRow);
                    }
                }

                dgrEvents.ItemsSource = TheEventLogSercurityDataSet.eventlogsecurity;

                PleaseWait.Close();
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Server Event Log Reports // Main Window // Process Day " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }

        private void expInsertRecords_Expanded(object sender, RoutedEventArgs e)
        {
            int intCounter;
            int intNumberOfRecords;
            bool blnFatalError = false;
            DateTime datTransactionDate;
            string strLogonName;
            string strItemAccessed;
            int intRecordsReturned;

            try
            {
                expInsertRecords.IsExpanded = false;

                PleaseWait PleaseWait = new PleaseWait();
                PleaseWait.Show();

                intNumberOfRecords = TheEventLogSercurityDataSet.eventlogsecurity.Rows.Count;

                if(intNumberOfRecords> 0)
                {
                    for(intCounter = 0; intCounter < intNumberOfRecords; intCounter++)
                    {
                        datTransactionDate = TheEventLogSercurityDataSet.eventlogsecurity[intCounter].TransactionDate;
                        strLogonName = TheEventLogSercurityDataSet.eventlogsecurity[intCounter].LogonName;
                        strItemAccessed = TheEventLogSercurityDataSet.eventlogsecurity[intCounter].ItemAccessed;

                        TheFindServerEventLogForReportsVerificationDataSet = TheEventLogClass.FindServerEventLogForReportsVerification(datTransactionDate, strLogonName, strItemAccessed);

                        intRecordsReturned = TheFindServerEventLogForReportsVerificationDataSet.FindServerEventLogForReportsVerification.Rows.Count;

                        if(intRecordsReturned < 1)
                        {
                            blnFatalError = TheEventLogClass.InsertServerEventLogForReports(datTransactionDate, strLogonName, strItemAccessed);

                            if (blnFatalError == true)
                                throw new Exception();
                        }
                    }
                }

                PleaseWait.Close();

                TheMessagesClass.InformationMessage("The Data Has Been Inserted");

                TheFindServerEventLogForReportsByDateRangeDataSet = TheEventLogClass.FindServerEventLogForReportsByDateRange(gdatStartDate, gdatEndDate);

                dgrEvents.ItemsSource = TheFindServerEventLogForReportsByDateRangeDataSet.FindServerEventLogForReportsByDateRange;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Server Event Log Reports // Main Window // Insert Records " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
    }
}
