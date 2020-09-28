<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://earth.google.com/kml/2.0" xmlns:set="http://exslt.org/sets" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:cysmil="urn:schemas-microsoft-com:time" xmlns:cyvml="urn:schemas-microsoft-com:vml" xmlns:cymath="urn:smiletime-cybarber-net:math" xmlns:gpx="http://www.topografix.com/GPX/1/1" xmlns:gml="http://www.opengis.net/gml" xmlns:gpx10="http://www.topografix.com/GPX/1/0" exclude-result-prefixes="gml gpx gpx10 cyvml cysmil" extension-element-prefixes="set  msxsl cymath">
  <xsl:output method="xml" version="1.0" encoding="utf-8" indent="yes" cdata-section-elements="description Snippet" media-type="application/vnd.google-earth.kml+xml"/>
  <xsl:strip-space elements="*"/>
  <!-- Kan ik de schema referentiest toevoegen aan het KML element: Ja als je het XSLT bestand 
	ValidateOnparse = false zet
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://schemas.opengis.net/gml/2.1.2/ http://schemas.opengis.net/gml/2.1.2/feature.xsd" 
-->
  <!-- GML files have the boundedBy Box element similar as Bounds element in GPX< KML has no such element yet -->
  <xsl:variable name="boundedByString" select="/gml:featureCollection/gml:boundedBy/gml:Box/gml:coordinates"/>
  <xsl:variable name="minlon">
    <xsl:value-of select="substring-before($boundedByString,',')"/>
  </xsl:variable>
  <xsl:variable name="minlat">
    <xsl:value-of select="substring-before(substring-after($boundedByString,','),' ')"/>
  </xsl:variable>
  <xsl:variable name="maxlon">
    <xsl:value-of select="substring-before(substring-after(substring-after($boundedByString,','),' '),',')"/>
  </xsl:variable>
  <xsl:variable name="maxlat">
    <xsl:value-of select="substring-after(substring-after(substring-after($boundedByString,','),' '),',')"/>
  </xsl:variable>
  <xsl:variable name="boxheight">3000</xsl:variable>
  <!-- Initializing -->
  <xsl:template match="/gml:featureCollection">
    <kml>
      <Document>
        <name>
          <xsl:text>GML2KML file</xsl:text>
        </name>
        <description>
          <xsl:text>This KML file was transformed by XSLT from a Polygon containing GML file</xsl:text>
        </description>
        <!-- START OF GLOBAL STYLE SECTION -->
        <Style id="wayPoint">
          <IconStyle>
            <Icon>
              <href>root://icons/palette-3.png</href>
              <y>96</y>
              <w>32</w>
              <h>32</h>
            </Icon>
          </IconStyle>
        </Style>
        <Style id="track">
          <IconStyle>
            <Icon>
              <href>root://icons/palette-3.png</href>
              <x>32</x>
              <w>32</w>
              <h>32</h>
            </Icon>
          </IconStyle>
        </Style>
        <Style id="route">
          <IconStyle>
            <Icon>
              <href>root://icons/palette-3.png</href>
              <y>96</y>
              <w>32</w>
              <h>32</h>
            </Icon>
          </IconStyle>
        </Style>
        <Style id="linestyle">
          <LineStyle>
            <color>64eeee17</color>
            <width>6</width>
          </LineStyle>
        </Style>
        <!-- END of STYLE SECTION -->
        <Placemark>
          <name>Bounds</name>
          <Snippet maxLines="3">
            The bounding box for the track defining the minimum, maximum longitudes, latitudes of the the track region.
          </Snippet>
          <LookAt>
            <longitude>
              <xsl:value-of select="($minlon + $maxlon) div 2"/>
            </longitude>
            <latitude>
              <xsl:value-of select="($minlat + $maxlat) div 2"/>
            </latitude>
            <range>30000</range>
            <tilt>0</tilt>
            <heading>0</heading>
          </LookAt>
          <styleUrl>#polygonStyle</styleUrl>
          <Style>
            <PolyStyle>
              <color>ff0000ff</color>
              <colorMode>random</colorMode>
            </PolyStyle>
            <IconStyle>
              <Icon>
                <href>root://icons/palette-3.png</href>
                <x>160</x>
                <y>64</y>
                <w>32</w>
                <h>32</h>
              </Icon>
            </IconStyle>
          </Style>
          <Polygon>
            <extrude>1</extrude>
            <tessellate>1</tessellate>
            <altitudeMode>clampedToGround</altitudeMode>
            <outerBoundaryIs>
              <LinearRing>
                <coordinates>
                  <xsl:value-of select="$minlon"/>,<xsl:value-of select="$minlat"/>,<xsl:value-of select="$boxheight"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$maxlon"/>,<xsl:value-of select="$minlat"/>,<xsl:value-of select="$boxheight"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$maxlon"/>,<xsl:value-of select="$maxlat"/>,<xsl:value-of select="$boxheight"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$minlon"/>,<xsl:value-of select="$maxlat"/>,<xsl:value-of select="$boxheight"/>
                  <xsl:text> </xsl:text>
                  <xsl:value-of select="$minlon"/>,<xsl:value-of select="$minlat"/>,<xsl:value-of select="$boxheight"/>
                  <xsl:text> </xsl:text>
                </coordinates>
              </LinearRing>
            </outerBoundaryIs>
          </Polygon>
        </Placemark>
        <!-- begin Waypoints folder
				-->
        <!--
				<Folder>
					<description>WayPoints Folder</description>
					<name>Waypoints</name>
					<open>0</open>
					<xsl:apply-templates select="gpx:wpt|gpx10:wpt"/>
				</Folder>
				-->
        <!--
				einde wayPoints Folder 
				
				-->
        <!-- begin tracks folder 
				-->
        <!--
				<Folder>
					<description>Tracks Folder</description>
					<name>Tracks</name>
					<visibility>1</visibility>
					<open>1</open>
					<xsl:apply-templates select="gpx:trk|gpx10:trk"/>
				</Folder>
				-->
        <!--
				einde TRACKs Hoofd Folder -->
        <!-- begin Routes Hoofd folder 
				-->
        <!--
				<Folder>
					<description>Routes Folder</description>
					<name>Routes</name>
					<visibility>1</visibility>
					<open>1</open>
					<xsl:apply-templates select="gpx:rte|gpx10:rte"/>
				</Folder>
				-->
        <!--
				 einde Routes Hoofd-Folder -->
        <!-- GroundOverlay and ScreenOverlay template folders
				-->
        <!--
				<Folder>
					<name>GroundOverlay</name>
					<GroundOverlay>
						<visibility>0</visibility>
						<refreshInterval>121</refreshInterval>
						<Icon>
							<href>example.jpg</href>
						</Icon>
						<drawOrder>0</drawOrder>
						<LatLonBox>
							<rotation>36.9994</rotation>
							<north>39.3082</north>
							<south>38.5209</south>
							<east>-95.1583</east>
							<west>-96.3874</west>
						</LatLonBox>
					</GroundOverlay>
				</Folder>
			-->
        <!--
				<Folder>
					<name>ScreenOverlay</name>
					<ScreenOverlay id="khScreenOverlay756">
						<description>This screen overlay uses fractional positioning to put the image in the exact center of the screen</description>
						<name>Simple crosshairs</name>
						<visibility>0</visibility>
						<refreshInterval>121</refreshInterval>
						<Icon>
							<href>myimage.jpg</href>
						</Icon>
						<overlayXY x="0.5" y="0.5" xunits="fraction" yunits="fraction"/>
						<screenXY x="0.5" y="0.5" xunits="fraction" yunits="fraction"/>
						<rotationXY x="0.5" y="0.5" xunits="fraction" yunits="fraction"/>
						<size x="0" y="0" xunits="pixels" yunits="pixels"/>
					</ScreenOverlay>
				</Folder>
				-->
        <Placemark>
          <name>A GML to KML transform</name>
          <Style>
            <PolyStyle>
              <color>b2ff00ff</color>
              <fill>1</fill>
              <outline>1</outline>
            </PolyStyle>
          </Style>
          <MultiGeometry>
            <extrude>1</extrude>
            <altitudeMode>relativeToGround</altitudeMode>
            <xsl:apply-templates select="gml:featureMember"/>
          </MultiGeometry>
        </Placemark>
      </Document>
    </kml>
  </xsl:template>
  <!-- einde Start-Up template  /level2/level2.Geometry/gml:MultiPolygon/gml:polygonMember/gml:Polygon | gml:featureMember/world_adm0/world_adm0.Geometry/gml:Polygon-->

  <!-- Todo: Instead of the gml:coordinates value call a template to insert an elevation value after each latitude coordinate, use template from phototrackstylesheet -->

  <xsl:template match="gml:featureMember">
    <Polygon>
      <extrude>1</extrude>
      <altitudeMode>relativeToGround</altitudeMode>
      <name>
        <xsl:value-of select=" world_adm0/world_adm0.NAME"/>
      </name>
      <description>
        <xsl:value-of select="level2/level2.FeatureID | world_adm0/world_adm0.FeatureID"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="world_adm0/world_adm0.REGION | level2/level2.CONTINENT"/>
        <br/>
        <xsl:value-of select=" world_adm0/world_adm0.NAME"/>
        <xsl:text> </xsl:text>
        <xsl:value-of select="world_adm0/world_adm0.GMI_CNTRY"/>
        <xsl:value-of select="gml:featureMember/level2/level2.TDWG_CODE"/>
      </description>
      <outerBoundaryIs>
        <LinearRing>
          <coordinates>
            <xsl:value-of select="level2/level2.Geometry/gml:MultiPolygon/gml:polygonMember/gml:Polygon/gml:outerBoundaryIs/gml:LinearRing/gml:coordinates | world_adm0/world_adm0.Geometry/gml:Polygon/gml:outerBoundaryIs/gml:LinearRing/gml:coordinates"/>
          </coordinates>
        </LinearRing>
      </outerBoundaryIs>
    </Polygon>
  </xsl:template>

  <xsl:template match="gml:polygonMember">
    <Polygon>
      <extrude>1</extrude>
      <altitudeMode>relativeToGround</altitudeMode>
      <outerBoundaryIs>
        <LinearRing>
          <coordinates>
            <xsl:value-of select="gml:Polygon/gml:outerBoundaryIs/gml:LinearRing/gml:coordinates"/>
          </coordinates>
        </LinearRing>
      </outerBoundaryIs>
    </Polygon>
  </xsl:template>
  <!-- Utility template to calculate the cumulative travelled meters for the toal track -->
  <xsl:template name="tracklength">
    <xsl:param name="presib">0</xsl:param>
    <xsl:param name="totaltravelled">0</xsl:param>
    <xsl:choose>
      <xsl:when test="$presib > 1">
        <xsl:variable name="distancedelta" select="cymath:distCosineLaw(number(/gpx:gpx/gpx:wpt[$presib - 1]/@lon|/gpx10:gpx/gpx10:wpt[$presib - 1]/@lon),number(/gpx:gpx/gpx:wpt[$presib - 1]/@lat|/gpx10:gpx/gpx10:wpt[$presib - 1]/@lat),number(/gpx:gpx/gpx:wpt[$presib]/@lon|/gpx10:gpx/gpx10:wpt[$presib - 1]/@lon),number(/gpx:gpx/gpx:wpt[$presib]/@lat|/gpx10:gpx/gpx10:wpt[$presib - 1]/@lat))"/>
        <xsl:call-template name="tracklength">
          <xsl:with-param name="presib" select="$presib - 1"/>
          <xsl:with-param name="totaltravelled" select="$totaltravelled + $distancedelta"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$totaltravelled"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <msxsl:script language="JScript" implements-prefix="cymath">
    <![CDATA[
		function abs(arg)	{ return Math.abs(arg);	}
		function acos(arg)	{ return Math.acos(arg);	}
		function asin(arg)	{ return Math.asin(arg);	}
		function atan(arg)	{ return Math.atan(arg);	}
		function atan2(arg1,arg2)	{ return Math.atan2(arg1,arg2);	}
		function sin(arg)	{ return Math.sin(arg);	}
		function tan(arg)	{ return Math.tan(arg);	}
		function cos(arg)	{ return Math.cos(arg);	}
		function exp(arg)	{ return Math.exp(arg);	}
		function power(base, power)	{ return Math.pow(base,power);	}
		function sqrt(arg)	{ return Math.sqrt(arg);	}
		function log(arg)	{ return Math.log(arg);	}
		function random(arg)	{ return Math.random(arg);	}
		function decdigits(arg1,arg2)	{ return arg1.toFixed(arg2);	}
		function sliceof(arg){ return arg.slice(0,-1); }
		function todaydate(arg){var vandaag = new Date().getYear(); var a="";var s = a.concat(vandaag,', ',arg,', http://cybarber.ath.cx/VMLXSLTview.xslt'); return(s);  }
 
         function degTodec(arg)
		 {
			 arg = arg.toUpperCase();						 		 
			 var dir = arg.slice(arg.length - 1);				 
			 arg = arg.slice(0,-1);												
			 var dms = arg.split(/[\s:,Â°Âºâ€²\'â€³\"]/);																	
			if (dms.length == 3)									 
			{                          			
				var sec = dms[2];                         
				if(!/[\.]/.test(sec))									
				{
					sl = dms[2].length;
					if(sl == 4)
					{
						sec = dms[2].substr(0,2).concat('.',dms[2].substr(2,2));  
					}else {
						sec = dms[2].substr(0,1).concat('.',dms[2].substr(1,2));
					}
				}
				var deg = dms[0]/1 + dms[1]/60 + sec/3600;  
			} else {                                        						
				if (/[NS]/.test(dir)) brng = '0' + brng;      		
				var deg = arg.slice(0,3)/1 + arg.slice(3,5)/60 + arg.slice(5)/3600;
			}
			
			if (/[WS]/.test(dir)) deg = -deg;              		 
		   return deg.toFixed(6);		//*Math.PI/180;							
		}
		
		function climbingMeters(elev1, elev2)
		{
		 var klim = elev2 - elev1;
		 klim>0?klim:klim=0;
		 return klim.toFixed(0); 
		}	
		
		function downhillMeters(elev1, elev2)
		{
		 var down = elev2 - elev1;
		 down<0?down:down=0;
		 return down.toFixed(0); 
		}
			
		function distCosineLaw (lon1,lat1,lon2,lat2)
			 {
			 var ratio = Math.PI / 180;
			 var R = 6371000; 
                 lon1 *= ratio
                 lat1 *=  ratio
                 lon2 *= ratio
                 lat2 *= ratio
			
			  var d =( Math.acos( Math.sin(lat1) * Math.sin(lat2)  +   Math.cos(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1) ) * R).toFixed(0) ;
			  return d;
			}
			
			
			function bearing (lon1, lat1, lon2, lat2)
			 {
				  var y = Math.sin(lon2 - lon1) * Math.cos(lat2);
				  var x = Math.cos(lat1) * Math.sin(lat2)  -  Math.sin(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1);
				  return Math.atan2(y, x);  
			}
			
			function bearingDeg (lon1, lat1, lon2, lat2)
                 {
                 var ratio = Math.PI / 180;
                 lon1 *= ratio
                 lat1 *=  ratio
                 lon2 *= ratio
                 lat2 *= ratio
				  var y = Math.sin(lon2 - lon1) * Math.cos(lat2);
				  var x = Math.cos(lat1) * Math.sin(lat2)  -  Math.sin(lat1) * Math.cos(lat2) * Math.cos(lon2 - lon1);
				  var result = Math.floor(Math.atan2(y, x) * 180 / Math.PI);
				  result<0?result = 360 + result:result; 
				   return result  
			}
			
			
			function radToBrng (rad)	 {		 return radToDegMinSec((rad+2*Math.PI) % (2*Math.PI));	}
			function radToDegMinSec(rad)	 {  return ((rad<0?'-':"") +  _dms(rad));	 }
			
			function _dms(rad)
			 {
				var d = Math.abs(rad * 180 / Math.PI);
				 var deg = Math.floor(d);
				 var min = Math.floor((d-deg)*60);
				 var sec = Math.round((d-deg-min/60)*3600);
		  // add leading zeros if required
				  if (deg<100) deg = '0' + deg; if (deg<10) deg = '0' + deg;
				  if (min<10) min = '0' + min;
				  if (sec<10) sec = '0' + sec;
				  return deg + '\u00B0' + min + '\u2032' + sec + '\u2033';
			}
	]]>
  </msxsl:script>
  <!-- template delete from result tree all built-in-templates generate text nodes -->
  <xsl:template match="*"/>
  <!--

    __   Script: William A Slabbekoorn - Oct 2003
   / _)      _        __   _    _  __ 
   |(_ \\ /||_) //_\ ||_)||_) ||_ ||_)
   \__) || ||__)|| | || \||__)||_ || \
	 
	  This is a MSXML4 XSLT transformation application which 
				converts ATOM-RSS feeds to an XHTML+SMIL+VML presentation.
				
				Feeditems can be combined in categories, which can be set by the user.
				
				Besides the VML and SMIL specifics, I made a user menu for setting
				transformation parameters for XSLT input source file and
				presentation colors. Three configurations can be save/loaded locally in userData stores.
				
				Copyright: Cybarber Web Services, William A Slabbekoorn, July-September 2003.
				http://cybarber.ath.cx/ATOMRSSfeedcombiner.html.
							 
	-->
</xsl:stylesheet>