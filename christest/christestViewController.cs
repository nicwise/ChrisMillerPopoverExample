using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;

namespace christest
{
	public partial class christestViewController : UIViewController
	{
		public christestViewController () : base ("christestViewController", null)
		{
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		//need to keep these around so they don't get GC'ed!
		DialogViewController dvc;
		UIPopoverController popOver;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.

			//DVC is the source one - the one we start with. Most of it's stuff is just setup

			dvc = new DialogViewController(null, false);

			var root = new RootElement("Hello there");
			var section = new Section();

			for(int i = 0; i < 10; i++)
			{
				//urgh, using lambas, so we need to capture a few things locally.
				//never sure if I have to do this with int's or not tho
				int locali = i;

				//make the local element, the one in the main list
				StyledStringElement localElement = null;
				localElement = new StyledStringElement("Hello " + i.ToString (), delegate {;

					//when you tap on the item, make a new root,
					// which is a radio list. Select item 1 (the second one) by default
					// but only cos I want to, no real reason :)

					RadioGroup radioGroup = new RadioGroup(1);

					//create 3 elements. This is stupid code copy and paste, they all do the same thing
					// I guess you'd make this from an array or database?
					var childroot = new RootElement("child", radioGroup)
					{
						new Section()
						{
							// I've made a custom RadioElement (down the bottom)
							// that, when selected, calls us back
							new CheckedRadioElement("First", delegate(CheckedRadioElement cre) {
								// .. and we dismiss the popover, grab the value out, and then tell the 
								// main dvc to reload/redisplay itself.
								popOver.Dismiss(true);
								localElement.Caption = cre.Caption;
								dvc.ReloadData();
							}),

							// these 2 are the same - just other data. Use a database or an array :)
							new CheckedRadioElement("Second", delegate(CheckedRadioElement cre) {
								popOver.Dismiss(true);
								localElement.Caption = cre.Caption;
								dvc.ReloadData();
							}),
							new CheckedRadioElement("Third", delegate(CheckedRadioElement cre) {
								popOver.Dismiss(true);
								localElement.Caption = cre.Caption;
								dvc.ReloadData();
							})
						}
					};

					//make the child DVC. This is the one which goes into the popover.
					// false on the end, 'cos we are not pushing it into a UINavigationController
					var childdvc = new DialogViewController(childroot, false);
					childdvc.Style = UITableViewStyle.Plain;



					//this does tho!
					//get the rect of the last section
					var newrootSize = childdvc.TableView.RectForSection (childroot.Count - 1);
					//and make that the size. Or 700... which ever is smaller.
					childdvc.ContentSizeForViewInPopover = new SizeF (300, Math.Min (newrootSize.Bottom, 700));

					//make the popover and set its size
					popOver = new UIPopoverController(childdvc);

					//and show the popover. We ask the tableview for the rect of the item we selected.
					// and in this case, we want to see it on the right (arrow == left)
					popOver.PresentFromRect(dvc.TableView.RectForRowAtIndexPath(localElement.IndexPath), dvc.TableView, UIPopoverArrowDirection.Left, true);


				}) {
					Accessory = UITableViewCellAccessory.DisclosureIndicator
				};

				section.Add (localElement);
			}

			root.Add (section);

			dvc.Root = root;

			//temp view is because a normal DVC wants to take over the whole screen, we I'm constraining it to a 300x300 area, which I think is about 
			// what you had. Thats the only reason tho.
			var tempView = new UIView(new RectangleF(10,10,300,300));
			tempView.AddSubview(dvc.TableView);


			View.Add (tempView);


		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return true;
		}
	}


}

namespace MonoTouch.Dialog
{
	
	public class CheckedRadioElement : RadioElement {

		public CheckedRadioElement (string caption, string group, Action<CheckedRadioElement> radioselected) : base (caption, group)
		{
			RadioSelected = radioselected;
		}
		
		public CheckedRadioElement (string caption, Action<CheckedRadioElement> radioselected) : base (caption)
		{
			RadioSelected = radioselected;
		}

		//This is important - this is no longer a normal RadioElement, so we need to
		// return the right cell key, so we get the right objects back.
		// in this case, we only have 1 type in the code above, but it's VERY important
		// if you have a Root with a few RadioElements and a few CheckedRadioElements
		// 'cos if you dont change it, you end up with reuse issues, drawing issues etc.

		static NSString newCellKey = new NSString("CheckedRadioElement");

		protected override NSString CellKey
		{
			get
			{
				return newCellKey;
			}
		}
		
		public Action<CheckedRadioElement> RadioSelected;

		// hook into the selected - pass it down, then call our handler.
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			base.Selected (dvc, tableView, indexPath);

			if (RadioSelected != null)
			{
				RadioSelected(this);
			}
		}
	}
	

}

