/**
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

			var lat = dataArray[i]['location']['latitude'];
			var lon = dataArray[i]['location']['longitude'];
			var geoLocation  = new google.maps.LatLng(lat, lon);
			var reference_file = dataArray[i]['reference_file'];
			var dialog_content = "Location: " + lat + "," + lon + "\n"
				+ "Timestamp: " + dataArray[i]['timestamp_local'] + " (local), " + dataArray[i]['timestamp_utc'] + " (UTC)" + "\n"
				+ "Filename: " + reference_file;
			
			var pixelLocation = projection.fromLatLngToDivPixel( geoLocation );

			var $point = $('<div '
								+'class="map-point" '
								+'id="p'+i+'"'
								+'title="'+i+'" '
								+'style="'
									+'width:8px; '
									+'height:8px; '
									+'left:'+pixelLocation.x+'px; '
									+'top:'+pixelLocation.y+'px; '
									+'position:absolute; '
									+'cursor:pointer; '
								+'" '
								+'data-dialog="'+dialog_content+'"'
							+'>'
								+'<img '
									+'src="assets/camera-photo.png" '
									+'style="position: absolute; top: -6px; left: -6px" '
								+'/>'
							+'</div>');
			
			// For zoom 8 and closer show a title above the marker icon
			/*if( zoom >= 8 ){
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
									+ reference_file
								+'</span>');
			}*/
			
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
	$('body').on('click', '.map-point', function( e ){
		$dialog.empty().append($(this).data('dialog'));
		$dialog.dialog('open');
	});
});