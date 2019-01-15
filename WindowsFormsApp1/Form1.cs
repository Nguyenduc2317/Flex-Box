using System;
using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Drawing;
using TSD = Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Tekla.Structures.Model;
using g3d = Tekla.Structures.Geometry3d;

//using ModelObject=Tekla.Structures.Model.ModelObject;
using TSM = Tekla.Structures.Model;

namespace FromDrawingToModel
{
    public partial class Form1 : Form
    {
        private DrawingHandler _drawingHandler;
        private Model _model;
        private Drawing _drawing;

        public Form1()
        {
            _drawingHandler = new DrawingHandler();
            _model = new Model();

            if (_model.GetConnectionStatus() &&
                _drawingHandler.GetConnectionStatus())
            {
                InitializeComponent();
            }
            else
                MessageBox.Show("Tekla Structures must be opened!");
        }

        //Pick a part from the drawing, to get its information
        //from the model
        private void PickObjectInDrawing_Click(object sender, EventArgs e)
        {
            try
            {
                _drawing = _drawingHandler.GetActiveDrawing();
                //Checks if any drawing is open
                if (_drawing != null)
                {
                    DrawingObject pickedObject;
                    ViewBase pickedView;
                    Tekla.Structures.Geometry3d.Point pickedPoint;

                    //Pick a part in the model 
                    Picker myPicker = _drawingHandler.GetPicker();
                    myPicker.PickObject("Pick a model object in the drawing", out pickedObject, out pickedView, out pickedPoint);
                    GetInfoFromDrawing(pickedObject);

                }
                else
                {
                    MessageBox.Show("A drawing must be opened in Tekla Structures model!");
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }
        }

        //This method gets the pciked object ID and selecs it in the model, showing
        //some of its information in the form
        private void GetInfoFromDrawing(DrawingObject pickedObject)
        {
            modelObjectTextBox.Clear();

            if (pickedObject != null)
            {
                TSD.ModelObject modelObjectInDrawing = pickedObject as TSD.ModelObject;
                if (modelObjectInDrawing != null)
                {
                    TSM.ModelObject modelObjectInModel = _model.SelectModelObject(modelObjectInDrawing.ModelIdentifier);
                    if (modelObjectInModel is TSM.RebarGroup || modelObjectInModel is TSM.Reinforcement)
                    {
                        Reinforcement beam = modelObjectInModel as Reinforcement;
                        if (beam != null)
                        { 
                            double radius = double.Parse(beam.RadiusValues[0].ToString());
                     
                            Solid sol = beam.GetSolid();
                            double maxX = sol.MaximumPoint.X;
                            double maxY = sol.MaximumPoint.Y;
                            double maxZ = sol.MaximumPoint.Z;

                            double minX = sol.MinimumPoint.X;
                            double minY = sol.MinimumPoint.Y;
                            double minZ = sol.MinimumPoint.Z;
                            // start vẽ polyline
                            Polyline MyPolyline;
                            PointList PolygonPoints = new PointList();
                            for (double i = maxY-radius; i <= maxY; i+=1)
                            {
                                PolygonPoints.Add(new g3d.Point(i,i));
                            }

                            DrawingObjectEnumerator views = _drawingHandler.GetActiveDrawing().GetSheet().GetAllViews();
                            while (views.MoveNext())
                            {
                                MyPolyline = new Polyline(views.Current as ViewBase, PolygonPoints);
                                MyPolyline.Insert();
                            }
                            GetBeamInfo(beam);
                            //end vẽ đường thẳng
                        }
                    }
                }
            }

        }
        private void GetBeamInfo(Reinforcement beam)
        {
            Solid sol = beam.GetSolid();
            double maxX = sol.MaximumPoint.X;
            double maxY = sol.MaximumPoint.Y;
            double maxZ = sol.MaximumPoint.Z;

            double minX = sol.MinimumPoint.X;
            double minY = sol.MinimumPoint.Y;
            double minZ = sol.MinimumPoint.Z;
            modelObjectTextBox.Text = TSM.ModelObject.ModelObjectEnum.BEAM.ToString() + Environment.NewLine +
                "Name: " + beam.Name + Environment.NewLine +
                "Id: " + beam.Identifier.ID.ToString() + Environment.NewLine +
                "maxX: " + maxX + Environment.NewLine +
                "maxY: " + maxY + Environment.NewLine +
                "maxZ: " + maxZ + Environment.NewLine +

                "minX: " + minX + Environment.NewLine +
                "minY: " + minY + Environment.NewLine +
                "minZ: " + minZ + Environment.NewLine;
        }
    }
}
