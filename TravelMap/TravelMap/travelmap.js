/**
 * Google Maps API V3 with jQuery
 * 
 * One Thousand Markers loaded fast using document.createDocumentFragment();
 * 
 * @author Nick Johnson {@link http://nickjohnson.com}
 * @link http://nickjohnson.com/b/
 */
$(document).ready(function(){

	if (typeof String.prototype.contains === 'undefined') { String.prototype.contains = function(it) { return this.indexOf(it) != -1; }; }

	var qs = (function(a) {
	    if (a == "") return {};
	    var b = {};
	    for (var i = 0; i < a.length; ++i)
	    {
	        var p=a[i].split('=', 2);
	        if (p.length == 1)
	            b[p[0]] = "";
	        else
	            b[p[0]] = decodeURIComponent(p[1].replace(/\+/g, " "));
	    }
	    return b;
	})(window.location.search.substr(1).split('&'));

	function get_marker_icon_url (n) {
		return "http://chart.apis.google.com/chart?chst=d_map_spin&chld=1|0|FFF600|15|_|"+n;
	}
	var icon_camera_url = "assets/camera-photo.png";
	var marker_icon_max_height = 48;
	var photo_icon_max_height = 18;

	var geocoder;
	geocoder = new google.maps.Geocoder();
	
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

    var dataArray = [];

    // HOOK: SET JSON DATA

    var markers = [];

	MyOverlay.prototype.draw = function()
	{
	    var projection = this.getProjection();
	    var zoom = this.getMap().getZoom();
	    var fragment = document.createDocumentFragment();
	    
	    this.markerLayer.empty(); // Empty any previous rendered markers

	    marker_groups = [];
		for (var i = 0; i < dataArray.length; i++) {
			// Determine a random location from the bounds set previously
			//var randomLatlng = new google.maps.LatLng(
			//		southWest.lat() + latSpan * Math.random(),
			//		southWest.lng() + lngSpan * Math.random()
			//);

			if (!dataArray[i]['location']) continue;

			if (!qs['all'] && !dataArray[i]['filename'].contains("PANO")) continue;

			var lat = dataArray[i]['location']['latitude'];
			var lon = dataArray[i]['location']['longitude'];
			var geoLocation  = new google.maps.LatLng(lat, lon);
			var reference_file = dataArray[i]['filename'];
			var thumbnail_url = "thumbnails/" + dataArray[i]['filename']; //"data:image/jpeg;charset=utf-8;base64," + dataArray[i]['thumbnail_base64'];
			var full_url = dataArray[i]['url_hosted'];
			var dialog_content = "<div class='col-lg-3 col-md-4 col-sm-6 col-xs-12'>"
				+ "<figure>"
				+ "<img src='" + full_url + "'>"
				+ "<figcaption>"
				+ "<b>Location:</b> " + lat + "," + lon + "<br>"
				+ "<b>Date:</b> " + dataArray[i]['timestamp_local'] + "<br>"
				//+ "<b>Timestamp:</b> " + dataArray[i]['timestamp_local'] + " (local), <br>" + dataArray[i]['timestamp_utc'] + " (UTC)" + "<br>"
				//+ "Filename: " + reference_file + "<br>"
				+ "</figcaption>"
				+ "</figure>"
				+ "</div>";
			
			
			var pixel_location = projection.fromLatLngToDivPixel( geoLocation );

			var icon_url;
			var icon_url_border_enabled;
			var icon_max_height;
			var grouping_enabled;
			if (zoom >= 10) {
				icon_url = thumbnail_url;
				icon_url_border_enabled = true;
				icon_max_height = photo_icon_max_height;
				grouping_enabled = false;
			}
			else {
				icon_url = get_marker_icon_url(1);
				icon_url_border_enabled = false;
				icon_max_height = marker_icon_max_height;
				grouping_enabled = true;
			}

			var marker_group = {
				'markers': [
					{
						'pixel_location': pixel_location,
						'coords': { 'lat': lat, 'lon': lon },
						'icon_url': icon_url,
					}
				],
				'dialog_content': dialog_content,
				'icon_url_border_enabled': icon_url_border_enabled,
				'count_photos': 1,
				'icon_max_height': icon_max_height,
			};
			var grouped = false;
			for (var j = 0; j < marker_groups.length; j++) {
				var stored_group = marker_groups[j];
				var dx = Math.abs(stored_group.markers[0].pixel_location.x - marker_group.markers[0].pixel_location.x);
				var dy = Math.abs(stored_group.markers[0].pixel_location.y - marker_group.markers[0].pixel_location.y);
				if (dx <= 15 && dy <= 15) {
					grouped = true;
					stored_group.dialog_content += marker_group.dialog_content;
					stored_group.count_photos += 1;
					if (grouping_enabled) {
						stored_group.markers[0].icon_url = get_marker_icon_url(stored_group.count_photos);
						stored_group.icon_url_border_enabled = false;
						stored_group.icon_max_height = marker_icon_max_height;
					}
					else {
						stored_group.markers = stored_group.markers.concat(marker_group.markers);
						stored_group.icon_url_border_enabled = true;
						stored_group.icon_max_height = photo_icon_max_height;
					}
				}
			}
			if (!grouped) {
				marker_groups.push(marker_group);
			}
		}

		for (var i = 0; i < marker_groups.length; i++) {
			var marker_group = marker_groups[i];

			for (var j = 0; j < marker_group.markers.length; j++) {

				var marker = marker_group.markers[j];

				var point_html = '<div '
									+'class="map-point icon_url_border_'+(marker_group.icon_url_border_enabled?"enabled":"disabled")+'" '
									+'id="p'+i+'" '
									+'title="'+i+'" '
									+'style="'
										+'width:8px; '
										+'height:8px; '
										+'left:'+marker.pixel_location.x+'px; '
										+'top:'+marker.pixel_location.y+'px; '
										+'position:absolute; '
										+'cursor:pointer; '
									+'" '
									+'data-dialog="'+marker_group.dialog_content+'" '
									+'data-lat="'+marker.coords.lat+'" '
									+'data-lon="'+marker.coords.lon+'" '
								+'>'
									+'<img '
										+'src="'+marker.icon_url+'" '
										+'style="position: absolute; top: -48px; left: -20px; max-height: '+marker_group.icon_max_height+'px;" '
									+'/>'
								+'</div>'
				;

				var $point = $(point_html);

				// Append the HTML to the fragment in memory
				fragment.appendChild( $point.get(0) );

			}
		}
		
		// Now append the entire fragment from memory onto the DOM
		this.markerLayer.append(fragment);
	};
	
	var myLatlng = new google.maps.LatLng(38.392303,-46.931067); // Jasper, IN
	
	var map = new google.maps.Map(document.getElementById("map-canvas"),
			{
				zoom: 3,
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
		var dialog_data = $(this).data('dialog');
		//$dialog.empty().append(dialog_data);
		var lat = $(this).data('lat');
		var lon = $(this).data('lon');
		geocoder.geocode(
			{ 'latLng': new google.maps.LatLng(lat, lon) },
			function (results, status) {
				var title = "";
				if (status === google.maps.GeocoderStatus.OK) {
					if (results[1]) {
						title = results[0]['formatted_address'];
					} else {
						title = 'Unknown address';
					}
				} else {
					title = 'Unknown location';
				}

				var winW = $(window).width() - 180;
				var winH = $(window).height() - 180;

				$('#map-modal .photos').empty().append(dialog_data);
				$('#map-modal .modal-title').text(title);
				$('#map-modal').modal();

				/*$dialog.dialog({
					title: title,
					height: winH,
					width: winW,
				});
				$dialog.dialog('open');*/
			}
		);
	});
});