   M S B A M L   `   `   `  ����C  ?MyGISApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:MyGISAppMyGISApp  5   	       MyGISApp.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Z VEsri.ArcGISRuntime, Version=10.2.6.0, Culture=neutral, PublicKeyToken=8fc3cc631e44ad866esri+http://schemas.esri.com/arcgis/runtime/2013  5   	   "localclr-namespace:MyGISApp  5   	     � Title$  
MainWindow��5	   	   $	��350��6   $	��525��6)   .��5
      � .+�5   
   )  #Esri.ArcGISRuntime.Controls.MapView  -   5          Name$ 	MyMapView��5      
   Map. 5      %  Esri.ArcGISRuntime.Controls.Map  -   5        	MyMap��   InitialViewpoint 5      1  +Esri.ArcGISRuntime.Controls.ViewpointExtent  5         XMin$ -9113800Q�6+      YMin$ 5077300Q�5   "      XMax$ -9078300Q�5   "      YMax$ 5091800Q�5   "      Layers. 5      :  4Esri.ArcGISRuntime.Layers.ArcGISTiledMapServiceLayer  %  Esri.ArcGISRuntime.Layers.Layer		   ID$	 BaseMap��62   
   
ServiceUri$V
 Phttp://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer��6?   5      5      �� 5   
   -   5         ��Name$ comboBox��5      $
��Left=�6%   $	��Top=�6@   $��	10,10,0,0q�5      $	��120��6&   @�5      �� 5      $��Streets��6#    �� 
IsSelected . 
 � Tag$V Phttp://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer��6G   �� 5      $
��Topo��6#   $T Nhttp://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer��62   �� 5      $��Imagery��6#   $S Mhttp://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer��65   5       �� 5!   
   
�������$	��300��6(   $��Right=�64   $��10q�6P   .+�5"      ��  �� Orientation$ 
Horizontal=�6   $��5q�62   .+�5#      �� $��Search:��6   �� 5$      -   5         ��Name$ SearchTextBox��5$      $	��100��61   $
��10,0q�6=   $��Right=�6K   $��Turtle��6n   �� 5%      -   5        $ ItemTypeComboBox��5%       �� SelectedValuePath$ Content��65   $	��120��6Q   .@�5&      �� $��Web map��6#    . 65   �� 5'      $��Feature service��6#   �� 5(      $��Basemap��6#   5)      5*      �� 5+      -   5         ��Name$ SearchButton��5+      $��Search ArcGIS.com��6+   $��20q�6G   6S   �� 5,      -   5         ��Name$ ResultsLIstBox��5,      $
��20,0q�6.   $	��200��6<   ��5.      �� 5/      .R�50      �� $ Vertical=�6%   .+�51      �� ��6(   �� *Title+�� 52      $��9(�6?   ��6(   �� *Owner+6?   53      54      56      �� 57      -   5         ��Name$ SnippetTextBlock��57      $�� ��62   $��50��6:   $��10q�6F   $
��Wrap=�6R   �� 58      -	   5        $��ThumbnailImage��58      $��UniformToFill=�6*   $	��200��6B   $	��133��6N   �� 59      -
   5        $ ShowMapButton��59      $��Show Map��6,   $��5q�6?   $	��150��6J   {�.  6h   5:      5=      5>      