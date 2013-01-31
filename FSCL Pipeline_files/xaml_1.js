var ARROW_IMG_SRC = '<Image Source="arrow.png"/>';
var ARROW_WIDTH = 33;
var ARROW_HEIGHT = 40;
var CONV_FACTOR = 96.0;

// Required for callbacks
var plugin;
var rootCanvas;
var pageCanvas;
var imgArrow;

var tooltip;

var SelectNodes = parent.SelectNodes

if (!window.SilverlightDisplay)
	window.SilverlightDisplay = {};

SilverlightDisplay.Drawing = function() 
{
}

SilverlightDisplay.Drawing.prototype =
{
	handleLoad: function(xaml_plugin, userContext, rootElement) 
	{
		plugin = xaml_plugin;
		imgArrow = null;
		this.tooltip = null;
		this.strLastShape = "";   // Name of the last shape that created an event
		this.shapeSelIndex = -1;
		this.shapeSel = null;
		
		this.root = rootElement;
		rootCanvas = rootElement;
		
		// Find the page's canvas
		for (var i = 0; i < rootCanvas.Children.Count; i++)
		{
			var child = rootCanvas.Children.GetItem(i);
			if (child.toString () == "Canvas" && child.Name == "D")
			{
				pageCanvas = child;
				break;
			}
		}
		
		this.scaleX = 1;
		this.scaleY = 1;
		var strTransform = '<ScaleTransform ScaleX="' + this.scaleX + '" ScaleY="' + this.scaleY + '"/>';
		rootCanvas.RenderTransform = plugin.Content.CreateFromXAML(strTransform);
		
		parent.viewMgr.origWidth = pageCanvas.Width;
		parent.viewMgr.origWH = pageCanvas.Width / pageCanvas.Height;
		
		rootElement.addEventListener ("LostFocus", Silverlight.createDelegate (this, this.handleLostFocus));
		
		this.addListenersToShapes (pageCanvas);
	},
	
	onResize: function(width, height)
	{
	    // Calculate the new scale
	    this.scaleX = width / rootCanvas.Width;
	    this.scaleY = height / rootCanvas.Height;
	    rootCanvas.RenderTransform.ScaleX = this.scaleX;
	    rootCanvas.RenderTransform.ScaleY = this.scaleY;
		
	    plugin.width = width;
	    plugin.height = height;
	},

	addListenersToShapes: function(canvasNode)
	{
	    // Add listeners to all the shapes with properties
	    for (var i = 0; i < canvasNode.Children.Count; i++)
	    {
	        var child = canvasNode.Children.GetItem(i);
	        if (child.toString () == "Canvas" && child.Name != "")
	        {
	            this.addListenersToShapes (child);    // Recursive call
            }
	        else
	        {
	            continue;
	        }
	        
			var shapeID = child.Name.substring (1, child.Name.length);	// Remove the _
			var shapeNode = parent.parent.FindShapeXML (pageID, shapeID);
			if(shapeNode)
			{
				var tmpProp = SelectNodes(shapeNode, "Prop").length > 0;
				var tmpNodes = false;
				if (!tmpProp)
					tmpNodes = SelectNodes(shapeNode, "Scratch/B/SolutionXML/HLURL:Hyperlinks/HLURL:Hyperlink");
				if (tmpProp || tmpNodes.length > 0)
				{
					child.addEventListener ("MouseEnter", 			Silverlight.createDelegate (this, this.handleMouseEnter));
					child.addEventListener ("MouseLeftButtonDown", 	Silverlight.createDelegate (this, this.handleMouseDown));
					child.addEventListener ("MouseLeftButtonUp", 	Silverlight.createDelegate (this, this.handleMouseUp));
					child.addEventListener ("MouseLeave", 			Silverlight.createDelegate (this, this.handleMouseLeave));
					
					if (tmpNodes.length > 0)
						child.Cursor = "Hand";
				}
			}
	    }
	},
	
	handleKeyDown: function(sender, eventArgs)
	{
	},
	
	getTextBlock: function(text, left, top)
	{
		return '<TextBlock Canvas.Left="' + left + '" Canvas.Top="' + top + '" FontFamily="Lucida Sans Unicode, Lucida Grande" FontSize="10.5" Text="' + text + '"/>';
	},
	
	removeTooltip: function()
	{
	    if (this.tooltip != null)
	    {
	        // Remove the tooltip from the Canvas object.
	        pageCanvas.Children.Remove(this.tooltip);
	    }
		this.tooltip = null;
	},
	
	addTooltip: function(sender, eventArgs, type)
	{
		// Determine whether the tooltip is created.
	    if (this.tooltip == null) 
		{
			var strHL, strProps;
			if (type == 'focus')
			{
			    strHL = parent.parent.strFocusHLTooltipText;
				strProps = parent.parent.strFocusPropsTooltipText;
			}
			else
			{
				strHL = parent.parent.strHLTooltipText;
				strProps = parent.parent.strPropsTooltipText;
			}
			
			// Build the tooltip string
			var shapeIDStr = sender.name;
			var shapeID = parseInt(shapeIDStr.substring(1, shapeIDStr.length));
			var shapeNode = parent.parent.FindShapeXML (pageID, shapeID);
			var txtBlocks = new Array(3);
			var border = 2;
			var iHeight = border;
			var iWidth = 0;
			
			if ( shapeNode != null )
			{
				var textNode = SelectNodes(shapeNode, "Text");
				if (textNode != null && textNode.length > 0)
				{
					var strNodeTitle = textNode[0].textContent;
					if (!strNodeTitle)
						strNodeTitle = textNode[0].text;
				    var strTxtBlock = this.getTextBlock (strNodeTitle, border, iHeight);
					txtBlocks[0] = plugin.content.createFromXaml (strTxtBlock, false);
					iHeight += txtBlocks[0].ActualHeight;
					iWidth = Math.max (iWidth, txtBlocks[0].ActualWidth + border * 2);
				}
				
				// Show the prop tooltip only when there are 
				// properties in the shape and the details pane is available
				var propNode = SelectNodes(shapeNode, "Prop");
				if (propNode != null && propNode.length > 0 &&
				    parent.frmToolbar.widgets != null && parent.frmToolbar.widgets.Details != null)
				{
				    var strTxtBlock = this.getTextBlock (strProps, border, iHeight);
					txtBlocks[1] = plugin.content.createFromXaml (strTxtBlock);
					iHeight += txtBlocks[1].ActualHeight;
					iWidth = Math.max (iWidth, txtBlocks[1].ActualWidth + border * 2);
				}
				
				var hlObj = parent.parent.GetHLAction (shapeNode, pageID, shapeID);
				if (hlObj != null && (hlObj.DoFunction.length > 0 || hlObj.Hyperlink.length > 0))
				{
				    var strTxtBlock = this.getTextBlock (strHL, border, iHeight);
					txtBlocks[2] = plugin.content.createFromXaml (strTxtBlock);
					iHeight += txtBlocks[2].ActualHeight;
					iWidth = Math.max (iWidth, txtBlocks[2].ActualWidth + border * 2);
				}
			}
			
			if(iWidth == 0)
			    return;	
			
			// Define the XAML fragment for the background of the tooltip.
	        var xamlFragment = '<Canvas Width="' + iWidth + '" Height="' + iHeight + '" Background="#FFFFE1">';
				xamlFragment +=   '<Canvas.RenderTransform><ScaleTransform ScaleX="' + (1/this.scaleX) + '" ScaleY="' + (1/this.scaleY) + '"/></Canvas.RenderTransform>';
				xamlFragment +=   '<Rectangle Width="' + iWidth + '" Height="' + iHeight + '" Stroke="Black">';
				xamlFragment +=			'<Rectangle.Fill>';
				xamlFragment +=					'<LinearGradientBrush StartPoint="0,0" EndPoint="0,1">';
				xamlFragment +=						'<GradientStop Color="White" Offset="0.0" />';
				xamlFragment +=						'<GradientStop Color="LightCyan" Offset="1.0" />';
				xamlFragment +=					'</LinearGradientBrush>';
				xamlFragment +=			'</Rectangle.Fill>';
				xamlFragment +=		'</Rectangle>';
				xamlFragment +=   '</Canvas>';
			   
			
	        // Create the XAML fragment for the tooltip.
	        this.tooltip = plugin.content.createFromXaml (xamlFragment, false);
	        
			// Position the tooltip at a relative x/y coordinate value.
			var cursorPosition = eventArgs.getPosition (pageCanvas);
	        this.tooltip["Canvas.Left"] = cursorPosition.x;
	        this.tooltip["Canvas.Top"] = cursorPosition.y + 20 * (1 / this.scaleY);
	        if(cursorPosition.x + this.tooltip.Width > pageCanvas.Width)
	        {
	            this.tooltip["Canvas.Left"] = pageCanvas.Width - this.tooltip.Width;
	        }
	        else if(cursorPosition.y + this.tooltip.Height + 20 > pageCanvas.Height)
	        {
	            this.tooltip["Canvas.Top"] = pageCanvas.Height - this.tooltip.Height;
	        }
	    }

		// Add the tooltip to the Canvas object.
		pageCanvas.Children.Add (this.tooltip);
		for (var i = 0; i < txtBlocks.length; i++)
        {
            if (txtBlocks[i] != null) 
                this.tooltip.Children.Add (txtBlocks[i]);
        }
        
        setTimeout("if(window.SilverlightObj.strLastShape == '" + sender.name + "') window.SilverlightObj.removeTooltip()", 2000);
	},
	
	handleMouseEnter: function(sender, eventArgs) 
	{
		if (this.tooltip != null)
	        this.removeTooltip ();
		    
		this.addTooltip (sender, eventArgs, 'mouse');
		this.strLastShape = sender.name;
	},
	
	handleMouseDown: function(sender, eventArgs) 
	{
          clickMenu ();      
	},
	
	handleDownloaderLoad: function(downloader, eventArgs)
	{
		var plugin = sender.getHost();
		
		var xamlPage = plugin.content.CreateFromXamlDownloader(downloader, "");
		
		var root = sender.findName("rootCanvas");
		root.Children.Add(xamlPage);
	},
	
	handleMouseUp: function(sender, eventArgs) 
	
	{
	    if(this.shapeSel != null)
	        this.shapeSel.Opacity = 1.0;
	    this.shapeSel = sender;
	    this.shapeSel.Opacity = 0.5;
	    
		var shapeIDStr = sender.name;
		var shapeID = parseInt(shapeIDStr.substring(1, shapeIDStr.length));
		parent.OnShapeClick(pageID, shapeID, null, eventArgs);
	},
	
	handleMouseLeave: function(sender, eventArgs) 
	{
		this.removeTooltip();
	},
	
	handleLostFocus: function(sender, eventArgs)
	{
	    if(this.shapeSel != null)
	        this.shapeSel.Opacity = 1.0;
	    this.shapeSel = null;
	},
	
	getIndexFromName: function(strName)
	{
	    var children = pageCanvas.Children;
	    for(var i = 0; i < children.Count; i++)
	    {
	        var child = children.GetItem(i);
	        if(child.Name == strName)
	            return i;
	    }
	    return -1;
	},
	
	isSelectable: function(shape)
	{
	    if(shape == null)
	        return false;
	    
	    if(shape.Name && shape.Name.length >= 2)
	    {
	        var shapeID = shape.Name.substring (1, shape.Name.length);	// Remove the _
	        var shapeNode = parent.parent.FindShapeXML (pageID, shapeID);
	        
	        if(shapeNode)
	        {
	            if (SelectNodes(shapeNode, "Prop").length > 0 || 
	                SelectNodes(shapeNode, "Scratch/B/SolutionXML/HLURL:Hyperlinks/HLURL:Hyperlink").length > 0)
	            {
		            return true;
	            }
	        }
	    }
	    
	    return false;
	},
	
	selectNextShape: function()
	{    
	    if(this.shapeSel == null)
	    {
	        this.shapeSel = this.nextShape(pageCanvas);
	    }
	    else
	    {
	        this.shapeSel.Opacity = 1.0;
	        this.shapeSel = this.nextShape(this.shapeSel);
	    }
	    
	    if(this.shapeSel)
	            this.shapeSel.Opacity = 0.5;            
	            
	    return this.shapeSel;
	},
	
	selectPrevShape: function()
	{	        
	    if(this.shapeSel == null)
	    {
	        this.shapeSel = this.prevShape(pageCanvas);
	    }
	    else
	    {
	        this.shapeSel.Opacity = 1.0;
	        this.shapeSel = this.prevShape(this.shapeSel);
	    }
	    
	    if(this.shapeSel)
	            this.shapeSel.Opacity = 0.5;            
	            
	    return this.shapeSel;
	},
	
	// Called when we gain the focus for the first time from tabbing
	startHandlingTabs: function(front)
	{
	    if(front)
	        return (this.selectNextShape() != null);
	    else
	        return (this.selectPrevShape() != null);
	},
	
	nextShape: function(startShape, currShape, lastVisited)  // currShape and currIndex should only be used by this function itself
	{
	    // Visit
	    if(currShape != null && startShape != currShape && this.isSelectable(currShape))
	    {
	        return currShape;
	    }
	    
	    if(currShape == null)
	    {
	        currShape = startShape;
	    }
	    
	    // Check this nodes children, making sure we don't recheck nodes as we traverse upwards
	    var shapeFound = null;
	    if(currShape.toString () == 'Canvas' && currShape.Children.Count != 0)
	    {
	        var start = 0;
	        if(lastVisited != null)
	        {
	            for(var i = 0; i < currShape.Children.Count; i++)
	            {
	                if(currShape.Children.GetItem(i).Name == lastVisited.Name)
	                {
	                    start = i + 1;
	                    break;
	                }
	            }
	        }
	        
	        for(var i = start; i < currShape.Children.Count; i++)
	        {
	            shapeFound = this.nextShape(startShape, currShape.Children.GetItem(i));
	            if(shapeFound)
	                return shapeFound;
	        }
	    }
	    
	    if(currShape == startShape && startShape.Name && startShape.Name != "D")   // We searched all child nodes and now need to search parent nodes
	    {
	        return this.nextShape(currShape.GetParent(), null, currShape);
	    }
	    
	    return null;
	},
	
	// Traverses upwards until it finds a possibility of child shapes
	prevShape: function(currShape)
	{
	    if(currShape == pageCanvas)
	    {
	        return this.RLVSearch(currShape);
	    }
	    
	    // Start traversing R->L starting to the left of the currShape
	    var parent = currShape.GetParent();
	    var startIndex;
	    while(currShape.Name != pageCanvas.Name)
	    {
	        for(startIndex = parent.Children.Count - 1; startIndex >= 0; startIndex--)
	        {
	            if(parent.Children.GetItem(startIndex).Name == currShape.Name)
	                break;
	        } 
	        startIndex--;
    	    if(startIndex != -1)
	            return this.RLVSearch(parent, startIndex)
	            
	        parent = parent.GetParent();
	        currShape = currShape.GetParent();
	    }
	    
	    return null;
	},
	
	// Traverses downwards until it finds a child shape, otherwise it tries to hop up again
	RLVSearch: function(currShape, startIndex, rootShape)
	{
	    if(currShape.toString () == 'Canvas' && currShape.Children.Count != 0)
	    {
	        // R to L search
	        if(startIndex == null)
	            startIndex = currShape.Children.Count - 1;
	        if(rootShape == null)
	            rootShape = currShape;
	            
	        for(var i = startIndex; i >= 0; i--)
	        {
	            var shapeFound = this.RLVSearch(currShape.Children.GetItem(i),null,rootShape);
	            if(shapeFound)
	                return shapeFound;
	        }
	        
	        if(this.isSelectable(currShape))
	            return currShape;
	    }
	    
	    // We can still hop up
	    if(currShape != pageCanvas && currShape == rootShape)
	        return this.prevShape(currShape);
	        
	        return null;
	},
	
	getBounds: function(shape)
	{
	    var strTag = shape.Tag;
	    var bounds = new Bounds();
	    bounds.x = getValueFromTag(strTag, "x");
	    bounds.y = getValueFromTag(strTag, "y");
	    bounds.width = getValueFromTag(strTag, "width");
	    bounds.height = getValueFromTag(strTag, "height");
	    
	    return bounds;
	}	
}

function getValueFromTag(tag, propName)
{
    var start = tag.indexOf('"' + propName + '"') + propName.length + 3;
    var end = tag.indexOf(',', start);
    if(end == -1) end = tag.indexOf('\\', start);
    return tag.substring(start,end);
}

function SetXAMLLocation(pageID, shapeID, pinX, pinY)
{
	clickMenu ();
	
    var xVal = CONV_FACTOR * pinX;
    var yVal = rootCanvas.Height - (CONV_FACTOR * pinY);

    var centeredX = xVal - Math.round(ARROW_WIDTH / 2);

    var xamlFragment =  '<Canvas Canvas.Left="' + centeredX + '" Canvas.Top="' + yVal + '">';
        xamlFragment += '<Canvas.RenderTransform><ScaleTransform ScaleX="' + (1 / rootCanvas.RenderTransform.ScaleX) + '" ScaleY="' + (1 / rootCanvas.RenderTransform.ScaleY) + '"/></Canvas.RenderTransform>';
        xamlFragment += ARROW_IMG_SRC;
        xamlFragment += '</Canvas>';
	
    if(imgArrow)
		rootCanvas.Children.Remove(imgArrow);

    imgArrow = plugin.content.createFromXaml(xamlFragment, false);
	
    var boolNeedToScroll = false;
    var doc = document;
    if( !( (xVal - ARROW_WIDTH / 2) > doc.body.scrollLeft && (xVal + ARROW_WIDTH / 2) < (doc.body.scrollLeft + doc.body.clientWidth) ))
	{
		boolNeedToScroll = true;
	}
	
	if( !( (yVal - ARROW_HEIGHT) > doc.body.scrollTop && (yVal + ARROW_HEIGHT) < (doc.body.scrollTop + doc.body.clientHeight) ))
	{
		boolNeedToScroll = true;
	}
	
	if( boolNeedToScroll == true )
	{
		window.scrollTo( xVal - doc.body.clientWidth / 2, yVal - doc.body.clientHeight / 2);
	}	
	
	rootCanvas.Children.Add(imgArrow);

    setTimeout("RemoveArrow()", 2000);
}

function RemoveArrow()
{
    if(imgArrow)
		rootCanvas.Children.Remove(imgArrow);

    imgArrow = null;
}

function XAMLZoomChange(size)
{
    if(size)
    {
	    if(size == "up")
	    {
		    size = zoomLast + 50;
	    }
	    else if(size == "down")
	    {
		    size = zoomLast - 50;
	    }
		
	    size = parseInt(size);
	    if(typeof(size) != "number")
		    size = 100;
    }
    else
    {
	    size = 100;
    }
    
    clickMenu ();
    
    viewMgr.zoomLast = size;
    
    var zoomFactor = size/100;
    
    var width = plugin.clientWidth;
    var height = plugin.clientHeight;
    
    var margin = parseInt(document.body.style.margin) * 2;
    
    var clientWidth = document.body.clientWidth;
	var clientHeight = document.body.clientHeight;
	
	// Get the scroll properties
	var newScrollLeft = document.body.scrollLeft;
	var newScrollTop = document.body.scrollTop;

	// ?Miscalculate the drawable width
	var winwidth = clientWidth - margin;
	var winheight = clientHeight - margin;

	// Calculate the ratio to turn pixel coordinates of the image into screen coordinates
	var widthRatio = winwidth / width;
	var heightRatio = winheight / height;
	
	// Calculate the new size and maintain aspect ratio
	if (widthRatio < heightRatio)
	{
		width = zoomFactor * winwidth;
		height = width / this.origWH;
	}
	else
	{
		height = zoomFactor * winheight;
		width = height * this.origWH;
	}
	
	drawing.onResize(Math.max(width, 1), Math.max(height, 1));
	
	// Resave the new size (also saved in this.zoomLast)
	this.sizeLast = size;
	
	// Calculate the center screen coordinate (includes offset for scrolling)
	var centerX = (zoomFactor / viewMgr.zoomFactor) * (newScrollLeft + (clientWidth / 2) - this.s.posLeft);
	var centerY = (zoomFactor / viewMgr.zoomFactor) * (newScrollTop + (clientHeight / 2) - this.s.posTop);
	
    viewMgr.zoomFactor = zoomFactor;
    
    // TODO Add padding if we are zoomed out, use less if zoomed in (width and height)
    if (width <= clientWidth)
	{
		this.s.posLeft = Math.max( 0, (clientWidth / 2) - (width / 2));
	}
	else
	{
		var left = centerX - (clientWidth / 2);
		if ( left >= 0 )
		{
			this.s.posLeft = 0;
			newScrollLeft = left;
		}
		else
		{
			this.s.posLeft = -left;
			newScrollLeft = 0;
		}
	}

	if (height <= clientHeight)
	{
		this.s.posTop = Math.max( 0, (clientHeight / 2) - (height / 2));
	}
	else
	{
		var top = centerY - (clientHeight / 2);
		if ( top >= 0 )
		{
			this.s.posTop = 0;
			newScrollTop = top;
		}
		else
		{
			this.s.posTop = -top;
			newScrollTop = 0;
		}
	}
	
    window.scrollTo(newScrollLeft, newScrollTop);
    
    // TODO Make the image visible
    this.s.visibility = "visible";
	
    var newXOffsetPercent = document.body.scrollLeft / plugin.width;
	var newYOffsetPercent = document.body.scrollTop / plugin.height;
	var newWidthPercent = document.body.clientWidth / plugin.width;
	var newHeightPercent = document.body.clientHeight / plugin.height;

	if (viewMgr.viewChanged)
	{
		viewMgr.viewChanged (newXOffsetPercent, newYOffsetPercent, newWidthPercent, newHeightPercent);
	}

	if (viewMgr.PostZoomProcessing)
	{
		viewMgr.PostZoomProcessing(size);
	}
}

function XAMLOnScroll()
{
    if (viewMgr.viewChanged)
	{
		var newXOffsetPercent = document.body.scrollLeft / plugin.width;
		var newYOffsetPercent = document.body.scrollTop / plugin.height;

		viewMgr.viewChanged (newXOffsetPercent, newYOffsetPercent, null, null);
	}
}

function XAMLOnResize ()
{
	if (viewMgr.zoomLast == 100)
	{
		viewMgr.Zoom(100);
	}

	if (viewMgr.viewChanged)
	{
		var newWidthPercent = document.body.clientWidth / plugin.width;
		var newHeightPercent = document.body.clientHeight / plugin.height;

		viewMgr.viewChanged (null, null, newWidthPercent, newHeightPercent);
	}
}

function XAMLSetView (xOffsetPercent, yOffsetPercent)
{
	var leftPixelOffset = xOffsetPercent * plugin.clientWidth;
	var topPixelOffset = yOffsetPercent * plugin.clientHeight;

	window.scrollTo (leftPixelOffset - this.s.posLeft, topPixelOffset - this.s.posTop);

	if (viewMgr.PostSetViewProcessing)
	{
		viewMgr.PostSetViewProcessing();
	}
}

//********************************Silverlight.Thumbnail*****************************************

SilverlightDisplay.Thumbnail = function() 
{
	this.plugin = null;
	this.rootCanvas = null;
	this.pageCanvas = null;
	this.callbackIndex = null;
	this.scale = null;
}

SilverlightDisplay.Thumbnail.prototype =
{
	handleLoad: function(rootElement) 
	{
		this.plugin = rootElement.getHost();
		this.rootCanvas = rootElement;
		
		// Find the page's canvas
		for (var i = 0; i < this.rootCanvas.Children.Count; i++)
		{
			var child = this.rootCanvas.Children.GetItem(i);
			if (child.toString () == "Canvas" && child.Name == "D")
			{
				this.pageCanvas = child;
				break;
			}
		}
		
		// Fit the canvas into the space given by the zoom window
		if(this.rootCanvas.Width > this.rootCanvas.Height)
		{
			this.scale = this.plugin.clientWidth / this.rootCanvas.Width;	
			this.rootCanvas['Canvas.Top'] = (this.rootCanvas.Width - this.rootCanvas.Height) / 2 * this.scale;
			this.rootCanvas['Canvas.Left'] = 0;
		}
		else
		{
			this.scale = this.plugin.clientHeight / this.rootCanvas.Height;
			this.rootCanvas['Canvas.Left'] = (this.rootCanvas.Height - this.rootCanvas.Width) / 2 * this.scale;
			this.rootCanvas['Canvas.Top'] = 0;
		}
		var strTransform = '<ScaleTransform ScaleX="' + this.scale + '" ScaleY="' + this.scale + '"/>';
		this.rootCanvas.RenderTransform = this.plugin.Content.CreateFromXAML(strTransform);
	}
}

function Bounds(x, y, width, height)
{
    this.x = x;
    this.y = y;
    this.width = width;
    this.height = height;
}

