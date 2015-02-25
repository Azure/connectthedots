 /*!
 * jQuery Bing Map 1.0
 * http://jquery-bing-maps.googlecode.com
 * Copyright (c) 2010 - 2012 Johan Säll Larsson
 * Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
 *
 * Depends:
 *		jquery.ui.bmap.js
 */
( function($) {

	$.extend($.ui.gmap.prototype, {
		 
		/**
		 * Gets the current position
		 * @param callback:function(position, status)
		 * @param geoPositionOptions:object, see https://developer.mozilla.org/en/XPCOM_Interface_Reference/nsIDOMGeoPositionOptions
		 */
		/*getCurrentPosition: function(a, b) {
			var c = this;
			if ( navigator.geolocation ) {
				navigator.geolocation.getCurrentPosition ( 
					function(d) {
						c._call(a, d, "OK");
					}, 
					function(error) {
						c._call(a, null, error);
					}, 
					b 
				);	
			} else {
				c._call(a, null, "NOT_SUPPORTED");
			}
		},*/
		
		/**
		 * Gets the current position
		 * @param callback:function(position, status)
		 * @param geoPositionOptions:object, see http://msdn.microsoft.com/en-us/library/hh125839.aspx
		 */
		getCurrentPosition: function(callback, geoPositionOptions) {
			var map = this.get('map'), opts = {} || geoPositionOptions;
			opts.successCallback = function(position) {
				callback(position, "OK");
			};
			opts.errorCallback = function(error) {
				callback(null, ( error.code === 5 ) ? 'NOT_SUPPORTED' : error.code);
			};
			var geolocation = this.get('geolocation', new Microsoft.Maps.GeoLocationProvider(map));
			geolocation.getCurrentPosition(opts)
		},
		
		/**
		 * Watches current position
		 * To clear watch, call navigator.geolocation.clearWatch(this.get('watch'));
		 * @param callback:function(position, status)
		 * @param geoPositionOptions:object, see https://developer.mozilla.org/en/XPCOM_Interface_Reference/nsIDOMGeoPositionOptions
		 */
		watchPosition: function(a, b) {
			var c = this;
			if ( navigator.geolocation ) {
				this.set('watch', navigator.geolocation.watchPosition ( 
					function(d) {
						c._call(a, d, "OK");
					}, 
					function(error) {
						c._call(a, null, error);
					}, 
					b 
				));	
			} else {
				c._call(a, null, "NOT_SUPPORTED");
			}
		},

		/**
		 * Clears any watches
		 */
		clearWatch: function() {
			if ( navigator.geolocation ) {
				navigator.geolocation.clearWatch(this.get('watch'));
			}
		},
		
		/**
		 * Page through the markers. Very simple version.
		 * @param prop:the marker property to show in display, defaults to title
		 */
		pagination: function(prop) {
			var $el = $("<div id='pagination' class='pagination shadow gradient rounded clearfix'><div class='lt btn back-btn'></div><div class='lt display'></div><div class='rt btn fwd-btn'></div></div>");
			var self = this, i = 0, prop = prop || 'title';
			self.set('p_nav', function(a, b) {
				if (a) {
					i = i + b;
					$el.find('.display').text(self.get('markers')[i][prop]);
					self.get('map').setView({'center': self.get('markers')[i].getLocation()});
				}
			});
			self.get('p_nav')(true, 0);
			$el.find('.back-btn').click(function() {
				self.get('p_nav')((i > 0), -1, this);
			});
			$el.find('.fwd-btn').click(function() {
				self.get('p_nav')((i < self.get('markers').length - 1), 1, this);
			});
			self.addControl($el, 0);			
		},
		
		/**
		 * Sets the data that is to be clustered and displayed on the map. All objects 
		 * must at minimium have a Latitude and Longitude properties. 
		 * @param arrayOfLocations:array - An array of objects that are to be mapped. 
		 */
		createCluster: function(arrayOfLocations) {
			var self = this;
			Microsoft.Maps.registerModule('clusterModule', 'http://www.bingmapsportal.com/Scripts/V7ClientSideClustering.js');
			Microsoft.Maps.loadModule('clusterModule', { callback: function() {
				self.clear('markers');
				var clusteredCollection = new ClusteredEntityCollection( self.get('map'), { 
					singlePinCallback: function(data) {
						var pushPin = new Microsoft.Maps.Pushpin(data._LatLong, {
							'icon': 'http://www.bingmapsportal.com/Content/nonclusteredpin.png',
							'anchor': new Microsoft.Maps.Point(8, 8)
						});
						pushPin.title = data.Title;
						pushPin.description = data.Description;
						pushPin.GridKey = data.GridKey;
						$(pushPin).click(function() {
							self.openInfoWindow({'title': this.target.title, 'description': this.target.description}, this);
						});
						return pushPin;
					},
					clusteredPinCallback: function(cluster, latlong) {
						return new Microsoft.Maps.Pushpin(latlong, {
							'icon': 'http://www.bingmapsportal.com/Content/clusteredpin.png',
							'anchor': new Microsoft.Maps.Point(8, 8)
						});
					}
				});
				clusteredCollection.SetData(arrayOfLocations);
			}});
		}
	
	});
	
} (jQuery) );