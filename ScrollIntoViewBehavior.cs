using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace TimeRecorder
{
    public class ScrollIntoViewBehavior : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                var grid = (sender as DataGrid);
                if (grid.SelectedItem != null)
                {
                    grid.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            grid.UpdateLayout();
                            grid.ScrollIntoView(grid.SelectedItem, null);
                        }));
                }
            }
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }
    }
}