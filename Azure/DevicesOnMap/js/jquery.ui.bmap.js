 /*!
 * jQuery Bing Map 1.0
 * http://jquery-bing-maps.googlecode.com
 * Copyright (c) 2010 - 2012 Johan Säll Larsson
 * Licensed under the MIT license: http://www.opensource.org/licenses/mit-license.php
 */
( function($) {
	
	/**
	 * Plugin
	 * @param namespace:string
	 * @param name:string
	 * @param base:object
	 */
	$.a = function(namespace, name, base ) {
		$[namespace] = $[namespace] || {};
		$[namespace][name] = function(options, element) {
			if ( arguments.length ) {
				this._setup(options, element);
			}
		};
		$[namespace][name].prototype = base;
		$.fn[name] = function(options) {
			var returnValue = this, args = Array.prototype.slice.call(arguments, 1), isMethodCall = typeof options === 'string';
			if ( isMethodCall && options.substring(0, 1) === "_" ) {
                return returnValue;
            }
			this.each(function() {
				var data = ( $.data(this, name) ) ? $.data(this, name) : $.data( this, name, new $[namespace][name](options, this) ); 
				if (isMethodCall) {
					returnValue = data[options].apply(data, args);
				}
			});
			return returnValue;
		};
	};
	
	$.a('ui', 'gmap', {
		
		/**
		 * Map options
		 * @see http://msdn.microsoft.com/en-us/library/gg427603.aspx
		 */
		options: { 'center': new Microsoft.Maps.Location(0.0, 0.0), 'zoom': 5 },
		
		/**
		 * Get or set options
		 * @param option:string
		 * @param value:object
		 */
		option: function(option, value) {
			if (!value) {
				return this.options[option];
			} else {
				this._u(option, value);
			}
		},
		
		/**
		 * Setup plugin basics, 
		 * Set the jQuery UI Widget this.element, so extensions will work on both plugins
		 * @param options:object
		 * @param element:object
		 */
		_setup: function(options, element) {
			this.el = $(element);
			jQuery.extend(this.options, options);
			this.options.center = this._latLng(this.options.center);
			this._create();
			if ( this._init ) {
				this._init();
			}
		},
		
		/**
		 * Create
		 */
		_create: function() {
			var self = this.instance = { el: this.el, map: new Microsoft.Maps.Map(this.el[0], this.options), markers: [], services: [], overlays: [] };
			this._call(this.options.callback, self.map);
			setTimeout( function() { self.el.trigger('init', self.map); }, 1);
		},
		
		/**
		 * Set map options
		 * @param option:string
		 * @param value:object
		 */
		_u: function(option, value) {
			var map = this.get('map');
			this.options[option] = value;
			map.setOptions(jQuery.extend(this.options, { 'center': map.getCenter(), 'mapTypeId': map.getMapTypeId(), 'zoom': map.getZoom() } ));
		},
		
		/**
		 * Adds a latitude longitude pair to the bounds.
		 * @param latlng:Microsoft.Maps.Location/string
		 */
		addBounds: function(latlng) {
			var map = this.get('map');
			var bounds = this.get('bounds', []);
			bounds.push(this._latLng(latlng));
			if ( bounds.length > 1 ) {
				map.setView({ 'bounds': Microsoft.Maps.LocationRect.fromLocations(bounds) });
			} else {
				map.setView({ 'zoom': this.get('map').getZoomRange().max, 'center': bounds[0] })
			}
		},
		
		/**
		 * Adds a custom control to the map
		 * @param element:jquery/node/string	
		 * @param position:int	 
		 */
		addControl: function(element, position) {
			var map = this.get('map');
			var node = $(this._unwrap(element));
			var css = {'position': 'absolute', 'z-index': 10 };
			if ( position < 3 ) {
				css.top = 0;
			} else if ( position > 2 && position < 6 ) {
				css.top = ( map.getHeight() - node.height() ) / 2;
			} else if ( position > 5 ) {
				css.bottom = 0;
			}
			if ( position == 0 || position == 3 || position == 6 ) {
				css.left = 0;
			} else if ( position == 1 || position == 4 || position == 7 ) {
				css.left = ( map.getWidth() - node.width() ) / 2;
			} else if ( position == 2 || position == 5 || position == 8 ) {
				css.right = 0;
			}
			node.css(css);
			this.el[0].appendChild(node[0]);
			return node;
		},
		
		/**
		 * Adds a Marker to the map
		 * @param pushpinOptions:Microsoft.Maps.PushpinOptions (optional)
		 * @param callback:function(Microsoft.Maps.Map, Microsoft.Maps.Pushpin) (optional)
		 * @param pushpin:Microsoft.Maps.Pushpin (optional)
		 * @return $(Microsoft.Maps.Pushpin)
		 * @see http://msdn.microsoft.com/en-us/library/gg427629.aspx
		 */
		addMarker: function(pushpinOptions, callback, pushpin) {
			var map = this.get('map');
			var pushpin = pushpin || Microsoft.Maps.Pushpin;
			pushpinOptions = (this._convert) ? this._convert('addMarker', pushpinOptions) : pushpinOptions;
			pushpinOptions.location = this._latLng(pushpinOptions.location);
			var pin = new pushpin(pushpinOptions.location, pushpinOptions);
			for ( property in pushpinOptions ) {
				pin[property] = pushpinOptions[property];
			}
			var markers = this.get('markers', []);
			if ( pin.getId() ) {
				markers[pin.getId()] = pin;
			} else {
				markers.push(pin);
			}
			if ( pushpinOptions.bounds ) {
				this.addBounds(pin.getLocation());
			}
			map.entities.push(pin);
			this._call(callback, map, pin);
			return $(pin);
		},
		
		/**
		 * Adds an Infobox to the map
		 * @param infoboxOptions:Microsoft.Maps.InfoboxOptions
		 * @param pushpin:Microsoft.Maps.Pushpin
		 * @param callback:function(Microsoft.Maps.Infobox) (optional)
		 * @return $(Microsoft.Maps.Infobox)
		 * @see http://msdn.microsoft.com/en-us/library/gg675210.aspx
		 */
		addInfoWindow: function(infoboxOptions, pushpin, callback) {
			var infobox = new Microsoft.Maps.Infobox(pushpin.getLocation(), infoboxOptions); 
			this.get('map').entities.push(infobox);
			this._call(callback, infobox);
			return $(infobox);
		},
		
		/**
		 * Triggers an InfoWindow to open
		 * @param infoboxOptions:Microsoft.Maps.InfoboxOptions
		 * @param pushpin:Microsoft.Maps.Pushpin (optional)
		 * @see http://msdn.microsoft.com/en-us/library/gg675210.aspx
		 */
		openInfoWindow: function(infoboxOptions, pushpin) {
			
			infoboxOptions.offset = infoboxOptions.offset || new Microsoft.Maps.Point(0,15);
			infoboxOptions.zIndex = infoboxOptions.zIndex || 99999;
			infoboxOptions = (this._convert) ? this._convert('openInfoWindow', infoboxOptions) : infoboxOptions;
			
			var pushpin = this._unwrap(pushpin), latlng;
			
			if ( !pushpin ) {
				latlng = infoboxOptions.location;
			} else if ( pushpin instanceof Microsoft.Maps.Pushpin ) {
				latlng = pushpin.getLocation();
			} else {
				latlng = pushpin.target.getLocation();
			}
			
			if ( this.get('iw') && this.get('map').entities.indexOf(this.get('iw')) > -1 ) {
				this.get('map').entities.remove(this.get('iw'));
			} 

			if ( infoboxOptions.htmlContent ) {
				if ( infoboxOptions.htmlContent instanceof Object ) {
					infoboxOptions.htmlContent = '<div class="Infobox">'+infoboxOptions.htmlContent.innerHTML+'</div>';
				} else {
					if ( infoboxOptions.htmlContent.match(RegExp(/<(?:"[^"]*"['"]*|'[^']*'['"]*|[^'">])+>/)) == null ) {
						infoboxOptions.description = infoboxOptions.htmlContent;
					}
				}
			}

			this.set('iw', new Microsoft.Maps.Infobox(latlng, infoboxOptions));
			this.get('map').entities.push(this.get('iw'));
			this.get('map').setView({ 'center': latlng });
			
		},
		
		/**
		 * Clears by type
		 * @param type:string i.e. markers, overlays, services
		 */
		clear: function(type) {
			this._c(this.get(type));
			this.set(type, []);
		},
		
		_c: function(obj) {
			for ( var property in obj ) {
				if ( obj.hasOwnProperty(property) ) {
					if ( obj[property] instanceof Microsoft.Maps.Pushpin || obj[property] instanceof Microsoft.Maps.Infobox || obj[property] instanceof Microsoft.Maps.Map ) {
						Microsoft.Maps.Events.removeHandler(obj[property]);
						this.get('map').entities.remove(obj[property]);
					} else if ( obj[property] instanceof Array ) {
						this._c(obj[property]);
					}
					obj[property] = null;
				}
			}
		},
		
		/**
		 * Returns the objects with a specific property and value, e.g. 'category', 'tags'
		 * @param context:string	in what context, e.g. 'markers' 
		 * @param options:object:
		 * 	property:string	the property to search within
		 * 	value:string
		 * 	delimiter:string (optional)
		 * @param callback:function(Microsoft.Maps.Pushpin, isFound:boolean)
		 */
		find: function(context, options, callback) {
			var ctx = this.get(context);
			for ( var property in ctx ) {
				if ( ctx.hasOwnProperty(property) ) {
					callback(ctx[property], (( options.delimiter && ctx[property][options.property] ) ? ( $.inArray(options.value, ctx[property][options.property].split(options.delimiter)) > -1 ) : ( ctx[property][options.property] === options.value )));
				}
			};
		},
		
		/**
		 * Helper function to check if a LatLng is within the viewport
		 * @param marker:Microsoft.Maps.Pushpin
		 */
		inViewport: function(marker) {
			return this.get('map').getBounds().contains(marker.getLocation());
		},

		/**
		 * Returns an instance property by key. Has the ability to set an object if the property does not exist
		 * @param key:string
		 * @param value:object(optional)
		 */
		get: function(key, value) {
			var instance = this.instance;
			if (!instance[key]) {
				if ( key.indexOf('>') > -1 ) {
					var array = key.replace(/ /g, '').split('>');
					for ( var i = 0; i < array.length; i++ ) {
						if ( !instance[array[i]] ) {
							if (value) {
								instance[array[i]] = ( (i + 1) < array.length ) ? [] : value;
							} else {
								return null;
							}
						}
						instance = instance[array[i]];
					}
					return instance;
				} else if ( value && !instance[key] ) {
					this.set(key, value);
				}
			}
			return instance[key];
		},
		
		/**
		 * Sets an instance property
		 * @param key:string
		 * @param value:object
		 */
		set: function(key, value) {
			this.instance[key] = value;
		},
		
		/**
		 * Destroys the plugin.
		 */
		destroy: function() {
			this.clear('markers');
			this.clear('services');
			this.clear('overlays'); 
			var instance = this.instance;
			instance.map.dispose();
			for ( var property in instance ) {
				instance[property] = null;
			}
			jQuery.removeData(this.el[0], 'gmap');
		},
		
		/**
		 * Helper method for calling a function
		 * @param callback
		 */
		_call: function(callback) {
			if ( callback && $.isFunction(callback) ) {
				callback.apply(this, Array.prototype.slice.call(arguments, 1));
			}
		},
		
		/**
		 * Helper method for Microsoft.Maps.Location
		 * @param latlng:string/Microsoft.Maps.Location
		 */
		_latLng: function(latlng) {
			if ( latlng instanceof Microsoft.Maps.Location ) {
				return latlng;
			} else {
				latlng = latlng.replace(/ /g,'').split(',');
				return new Microsoft.Maps.Location(latlng[0], latlng[1]);
			}
		},
		
		/**
		 * Helper method for unwrapping jQuery/DOM/string elements
		 * @param element:string/node/jQuery
		 */
		_unwrap: function(element) {
			if ( !element ) {
				return null;
			} else if ( element instanceof jQuery ) {
				return element[0];
			} else if ( element instanceof Object ) {
				return element;
			}
			return $('#'+element)[0];
		}
		
	});
	
	jQuery.fn.extend( {
		
		click: function(a, b) { 
			return this.addEventListener('click', a, b);
		},
		
		rightclick: function(a, b) {
			return this.addEventListener('rightclick', a, b);
		},
		
		dblclick: function(a, b) {
			return this.addEventListener('dblclick', a, b);
		},
		
		mouseover: function(a, b) {
			return this.addEventListener('mouseover', a, b);
		},
		
		mouseout: function(a, b) {
			return this.addEventListener('mouseout', a, b);
		},
		
		drag: function(a) {
			return this.addEventListener('drag', a);
		},
		
		dragend: function(a) {
			return this.addEventListener('dragend', a);
		},
		
		triggerEvent: function(a) {
			Microsoft.Maps.Events.invoke(this[0], a);		
		},

		addEventListener: function(a, b, c) {
			if ( this[0] instanceof Microsoft.Maps.Pushpin || this[0] instanceof Microsoft.Maps.Infobox || this[0] instanceof Microsoft.Maps.Map ) {
				Microsoft.Maps.Events.addHandler(this[0], a, b);
			} else {
				if (c) {
					this.bind(a, b, c);
				} else {
					this.bind(a, b);
				}	
			}
			return this;
		}
		
	});
	
} (jQuery) );