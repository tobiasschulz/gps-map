﻿/**
 * Google Maps API V3 with jQuery
 * 
 * One Thousand Markers loaded fast using document.createDocumentFragment();
 * 
 * @author Nick Johnson {@link http://nickjohnson.com}
 * @link http://nickjohnson.com/b/
 */
$(document).ready(function(){
	
	var southWest = new google.maps.LatLng(40.744656,-74.005966); // Los Angeles, CA
	var northEast = new google.maps.LatLng(34.052234,-118.243685); // New York, NY
	var lngSpan = northEast.lng() - southWest.lng();
	var latSpan = northEast.lat() - southWest.lat();
		
	function MyOverlay( options )
	{
	    this.setValues( options );
	    this.markerLayer = $('<div />').addClass('overlay');
	};

	// MyOverlay is derived from google.maps.OverlayView
	MyOverlay.prototype = new google.maps.OverlayView;

	MyOverlay.prototype.onAdd = function()
	{
	    var $pane = $(this.getPanes().overlayImage); // Pane 4
        $pane.append( this.markerLayer );
	};

	MyOverlay.prototype.onRemove = function()
	{
		this.markerLayer.remove();
	};

	MyOverlay.prototype.draw = function()
	{
	    var projection = this.getProjection();
	    var zoom = this.getMap().getZoom();
	    var fragment = document.createDocumentFragment();
	    
	    this.markerLayer.empty(); // Empty any previous rendered markers

	    var dataArray = [];

	    // HOOK: SET JSON DATA
	    
		for(var i = 0; i < dataArray.length; i++){
			// Determine a random location from the bounds set previously
			//var randomLatlng = new google.maps.LatLng(
			//		southWest.lat() + latSpan * Math.random(),
			//		southWest.lng() + lngSpan * Math.random()
			//);

			var latLng  = new google.maps.LatLng(
					dataArray[i]['latitude'],
					dataArray[i]['longitude']
			);
			
			var randomLocation = projection.fromLatLngToDivPixel( coordinateArray[i] );

			var $point = $('<div '
								+'class="map-point" '
								+'id="p'+i+'"'
								+'title="'+i+'" '
								+'style="'
									+'width:8px; '
									+'height:8px; '
									+'left:'+randomLocation.x+'px; '
									+'top:'+randomLocation.y+'px; '
									+'position:absolute; '
									+'cursor:pointer; '
							+'">'
								+'<img '
									+'src="fish-mini-20.png" '
									+'style="position: absolute; top: -6px; left: -6px" '
								+'/>'
							+'</div>');
			
			// For zoom 8 and closer show a title above the marker icon
			if( zoom >= 8 ){
				$point.append('<span '
								+'style="'
									+'position:absolute; '
									+'top:-22px; '
									+'left:-37px; '
									+'width:75px; '
									+'background-color:#fff; '
									+'border:solid 1px #000; '
									+'font-family: Arial, Helvetica, sans-serif; '
									+'font-size:10px; '
									+'text-align:center; '
								+'">'
									+'Custom ID '+i
								+'</span>');
			}
			
			// Append the HTML to the fragment in memory
			fragment.appendChild( $point.get(0) );
		}
		
		// Now append the entire fragment from memory onto the DOM
		this.markerLayer.append(fragment);
	};
	
	var myLatlng = new google.maps.LatLng(38.392303,-86.931067); // Jasper, IN
	
	var map = new google.maps.Map(document.getElementById("map-canvas"),
			{
				zoom: 4,
				center: myLatlng,
				mapTypeId: google.maps.MapTypeId.ROADMAP
			});
	
	var OverlayMap = new MyOverlay( { map: map } );
	
	// A simple jQuery UI dialog for each marker
	var $dialog = $('<div id="dialog"></div>')
		.append('body')
		.dialog({
			autoOpen:false,
			width: 300,
			height: 200
		});

	$('#dialog').bind( "dialogopen", function( event, ui ){
		if($('body #dialog')){
			$dialog.parent().appendTo('#map-canvas');
		}
	});
	
	// Make sure to use live because the markers are rendered by javascript after initial DOM load
	$('.map-point').live('click',function( e ){
		$dialog.empty().append($(this).attr('id'));
		$dialog.dialog('open');
	});
});