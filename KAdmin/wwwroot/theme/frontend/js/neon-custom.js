/**
 *	Neon Main JavaScript File
 *
 *	Theme by: www.laborator.co
 **/

;(function($, window, undefined){

	"use strict";
	
	$(document).ready(function()
	{	
		//$("nav .selectnav").prependTo( $("nav .mobile-menu") );
		
		// Remove last option (from search box)
		//$("nav .selectnav").find('option:last').remove();
		
		
		// Menu Hover
		var $main_menu = $("nav.site-nav .main-menu");
		
			// Mobile Menu
			var $mobile_menu = $main_menu.clone();
			
			$("body").prepend( $mobile_menu );
			$mobile_menu.removeClass('hidden-xs main-menu').addClass('mobile-menu');
			
			$(".menu-trigger").on('click', function(ev)
			{
				ev.preventDefault();
				
				$mobile_menu.slideToggle();
			});
			
			
		
			// Sub Menus
			$main_menu.find('li:has(> ul)').addClass("has-sub").each(function(i, el)
			{
				var $this = $(el),
					$sub = $this.children('ul'),
					$sub_sub = $sub.find('> li > ul');
				
				
				$sub.addClass('visible');
				
				if($sub_sub.length)
				{
					$sub_sub.removeClass('visible');
				}
				
				$this.data('sub-height', $sub.outerHeight());
				
				if($sub_sub.length)
				{
					$sub_sub.addClass('visible');
				}
			});
			
			$main_menu.find('.visible').removeClass('visible');
			
			// First Level
			$main_menu.find('> li:has(> ul)').addClass('is-root').each(function(i, el)
			{
				var $this = $(el),
					$sub = $this.children('ul');
				
				TweenMax.set($sub, {css: {opacity: 0}});
				
				$this.hoverIntent({
					over: function()
					{
						TweenMax.to($sub, 0.3, {css: {opacity: 1}, onStart: function()
						{
							$this.addClass('hover');
						}});
						
						$sub.show();
					},
					
					out: function()
					{
						$sub.hide();
					},
					
					timeout: 300,
					
					interval: 0
				});
				
				$this.on('mouseleave', function()
				{
					TweenMax.to($sub, 0.15, {css: {opacity: 0}, onComplete: function()
					{
						$this.removeClass('hover');
					}});
				});
			});
			
			$main_menu.find('li:has(> ul)').not('.is-root').each(function(i, el)
			{
				var $this = $(el),
					$sub = $this.children('ul'),
					height = $this.data('sub-height');
				
				
				$this.hoverIntent({
					over: function()
					{
						if( ! $sub.is(':visible'))
							$sub.css({height: 0}).show();
							
						TweenMax.to($sub, .2, {css: {height: height}, ease: Sine.easeOut, onComplete: function()
						{
							$sub.attr('style', '').addClass('visible');
						}});
					},
					
					out: function()
					{
						TweenMax.to($sub, .2, {css: {height: 0}, ease: Sine.easeOut, onComplete: function()
						{
							$sub.attr('style', '').removeClass('visible');
						}});
					},
					
					interval: 150
				});
			});
			
			
			
		
		// Menu Search
		var $main_menu = $(".main-menu"),
			$menu_search = $main_menu.find('li.search .search-form .form-control');
		
		$main_menu.find('li.search a').click(function(ev)
		{
			ev.preventDefault();
			
			$main_menu.addClass('search-active');
			setTimeout(function(){ $menu_search.focus(); }, 180);
		});
		
		$menu_search.on('blur', function(ev)
		{
			$main_menu.removeClass('search-active');
		});
		
		
		// Clients Logos Carousel
		$(".client-logos").carousel();
		
		
		// Testimonials Carousel
		$(".testimonials").each(function(i, el){
			var $this = $(el),
				items_count = $this.find('.item').length;
			
			$this.carousel({
				//interval: 7000
			});
			
			if(items_count > 1)
			{
				var $tnav = $('<div class="testimonials-nav"></div>');
				
				for(var i=0; i<items_count; i++)
				{
					$tnav.append('<a href="#" data-index="' + i + '"></a>');
				}
				
				$tnav.find('a:first').addClass('active');
				$this.append($tnav);
				
				
				$tnav.on('click', 'a', function(ev)
				{
					ev.preventDefault();
					
					var index = $(this).data('index');
					
					$this.carousel(index);
				});
				
				$this.on('slide.bs.carousel', function(ev)
				{
					var index = $(ev.relatedTarget).index();
					
					$tnav.find('a').removeClass('active');
					$($tnav.find('a').get(index)).addClass('active');
				});
			}
		});
		
		
		// Alternate select box
		$(".alt-select-field").each(function(i, el)
		{
			var $this = $(el),
				$label = $this.find('.btn-label'),
				$items = $this.find('.dropdown-menu li'),
				$default = $('<li><a href="#" data-is-default="1">' + $label.html() + '</a></li>'),
				$current;
			
			$label.data('default-text', $label.html());
			$current = $items.filter('.active');
			
			$items.parent().prepend($default);
			
			if($current.length)
			{
				$label.html( $current.find('a').html() );
			}
			
			// Events
			$this.find('.dropdown-menu').on('click', 'li a', function(ev)
			{
				ev.preventDefault();
				
				var $item = $(this),
					$li = $item.parent(),
					isDefault = $item.data('is-default') ? true : false;
				
				$this.find('.dropdown-menu li').not($li).removeClass('active');
				$li.addClass('active');
				
				$this.trigger('change', [{isDefault: isDefault, el: $item, elParent: $li, index: $li.index()}]);
				
				// Set Current
				$current = $this.find('.dropdown-menu li.active');
				$label.html( $current.find('a').html() );
				
				if(isDefault)
				{
					$current.addClass('hidden');
				}
				else
				{
					$this.find('a[data-is-default]').parent().removeClass('hidden');
				}
			});
		});
		
		
		// Slides
		var $sliders = $(".slides");
		
		if($sliders.length && $.isFunction($.fn.neonSlider))
		{
			$sliders.neonSlider({
				itemSelector: '.slide',
				autoSwitch: 5
			});
		}
		
		
		// Enable/Disable Resizable Event
		var wid = 0;
		
		$(window).resize(function() {
			clearTimeout(wid);
			wid = setTimeout(trigger_resizable, 200);
		});
		
	});

})(jQuery, window);