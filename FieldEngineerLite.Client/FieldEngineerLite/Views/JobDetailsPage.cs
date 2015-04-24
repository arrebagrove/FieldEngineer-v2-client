﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ContosoAuto.Helpers;
using ContosoAuto.Models;

namespace ContosoAuto.Views
{	
    public class JobDetailsPage : ContentPage
    {        
        public JobDetailsPage()
        {
            TableSection mainSection = new TableSection("Customer Details");     
            
            mainSection.Add(new DataElementCell("CustomerName", "Customer"));
            mainSection.Add(new DataElementCell("Title", "Customer Notes"));
            mainSection.Add(new DataElementCell("CustomerAddress", "Address") { Height = 60 });
            mainSection.Add(new DataElementCell("CustomerPhoneNumber", "Telephone"));

            var statusCell = new DataElementCell("Status");
            statusCell.ValueLabel.SetBinding<Job>(Label.TextColorProperty, job => job.Status, converter: new JobStatusToColorConverter());
            mainSection.Add(statusCell);

            var workSection = new TableSection("Work Performed");            
            var workRowTemplate = new DataTemplate(typeof(SwitchCell));            
            workRowTemplate.SetBinding(SwitchCell.TextProperty, "Name");
            workRowTemplate.SetBinding(SwitchCell.OnProperty, "Completed");

			// I don't have images working on Android yet
			//if (Device.OS == TargetPlatform.iOS) 			
			//	equipmentRowTemplate.SetBinding (ImageCell.ImageSourceProperty, "ThumbImage");

            var workListView = new ListView {
                RowHeight = 50,
                ItemTemplate = workRowTemplate
            };
            workListView.SetBinding<Job>(ListView.ItemsSourceProperty, job => job.Items);            

            var workCell = new ViewCell { View = workListView };            
            workSection.Add(workCell);

            var actionsSection = new TableSection("Actions");
            
            TextCell completeJob = new TextCell { 
                Text = "Mark Completed",
				TextColor = AppStyle.DefaultActionColor
            };            
            completeJob.Tapped += async delegate {
                await this.CompleteJobAsync();
            };
            actionsSection.Add(completeJob);
            
            var table = new TableView
            {
                //BackgroundColor = Color.Transparent,
                Intent = TableIntent.Form,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HasUnevenRows = true,
                Root = new TableRoot("Root")
                {
                    mainSection, workSection, actionsSection, 
                }
            };
            table.SetBinding<Job>(TableView.BackgroundColorProperty, job => job.Status, converter: new JobStatusToColorConverter(useLightTheme: true));
            
            this.Title = "Appointment Details";

            //this.BackgroundImage = "Fabrikam-568h.png";
            //var background = new Image() { Aspect = Aspect.AspectFit };
            //background.Source = ImageSource.FromFile("Fabrikam-568h");

            this.Content = new ScrollView {
                //VerticalOptions = LayoutOptions.Fill,
                Orientation = ScrollOrientation.Vertical,

                Content = new StackLayout
                {
                    //BackgroundColor = Color.Transparent,
                    Orientation = StackOrientation.Vertical,
                    Children = { new JobHeaderView(), table }

                }
            };

            this.BindingContextChanged += delegate
            {
                if (SelectedJob != null && SelectedJob.Items != null)
                    workCell.Height = SelectedJob.Items.Count * workListView.RowHeight;
            };
        }

        private Job SelectedJob
        {
            get { return this.BindingContext as Job; }
        }

        private async Task CompleteJobAsync()
        {
            var job = this.SelectedJob;
            job.WorkPerformed = "";
            foreach(WorkItem e in job.Items) 
            {
              if (e.Completed) 
              {
                    job.WorkPerformed += " " + e.Name + ";";
              }
            }
            await App.JobService.CompleteJobAsync (job);

            // Force a refresh
            this.BindingContext = null;
            this.BindingContext = job;
        }

        private class DataElementCell : ViewCell
        {
            public Label DescriptionLabel { get; set; }
            public Label ValueLabel { get; set; }

            public DataElementCell(string property, string propertyDescription = null)
            {
                DescriptionLabel = new Label {
                    Text = propertyDescription ?? property,
                    Font = AppStyle.DefaultFont.WithAttributes(FontAttributes.Bold),
                    WidthRequest = 150,
                    VerticalOptions = LayoutOptions.CenterAndExpand                
                };

                ValueLabel = new Label {
                    Font = AppStyle.DefaultFont,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    XAlign = TextAlignment.End
                };
                ValueLabel.SetBinding(Label.TextProperty, property);

                this.View = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal,
                    Padding = 10,
                    Children = { DescriptionLabel, ValueLabel }
                };
            }
        }   
    }
}
