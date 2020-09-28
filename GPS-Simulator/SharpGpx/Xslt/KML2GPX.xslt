<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.topografix.com/GPX/1/1" xmlns:kml="http://earth.google.com/kml/2.0" xmlns:set="http://exslt.org/sets" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:cymath="urn:smiletime-cybarber-net:math" xmlns:gpx10="http://www.topografix.com/GPX/1/0" xmlns:gpx_style="http://www.topografix.com/GPX/gpx_style/0/1" xmlns:gpx_overlay="http://www.topografix.com/GPX/gpx_overlay/0/1"
                xmlns:topografix="http://www.topografix.com/GPX/private/topografix/0/2" xmlns:offroute="http://www.topografix.com/GPX/private/Offroute/0/1" xmlns:navaid="http://navaid.com/GPX/NAVAID/0/4"
                exclude-result-prefixes="kml gpx10 gpx_style gpx_overlay topografix offroute navaid" extension-element-prefixes="set  msxsl cymath">
  <xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes" cdata-section-elements="desc"/>
  <xsl:strip-space elements="*"/>
  <xsl:preserve-space elements="description"/>
  <xsl:param name="processpoints" select="'dopoints'"/>
  <!-- not to process points folder 'nopoints' -->
  <xsl:param name="processpath" select="'nopath'"/>
  <!-- to process path-placemark 'dopath' -->
  <xsl:variable name="placemarks" select="//kml:Placemark[kml:Point]"/>
  <xsl:variable name="totalplacemarks" select="count(msxsl:node-set($placemarks))"/>
  <xsl:variable name="multi500">
    <xsl:choose>
      <xsl:when test="floor($totalplacemarks div 500) &gt; 0">
        <xsl:value-of select="floor($totalplacemarks div 500)"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="1"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- stonden in onderstaand template -->
  <!-- Initializing -->
  <xsl:template match="/kml:kml">

    <!-- Matches all Point Placemarks for Boundery Box (bounds element) longitude, latitude calculation-->


    <xsl:variable name="minlon">
      <xsl:for-each select="msxsl:node-set($placemarks)/kml:Point">
        <xsl:sort select="substring-before(kml:coordinates,',')" data-type="number"/>
        <xsl:if test="position()=1">
          <xsl:value-of select="substring-before(kml:coordinates,',')"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="minlat">
      <xsl:for-each select="msxsl:node-set($placemarks)/kml:Point">
        <xsl:sort select="substring-before(substring-after(kml:coordinates,','),',')" data-type="number"/>
        <xsl:if test="position()=1">
          <xsl:value-of select="substring-before(substring-after(kml:coordinates,','),',')"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="maxlon">
      <xsl:for-each select="msxsl:node-set($placemarks)/kml:Point">
        <xsl:sort select="substring-before(kml:coordinates,',')" data-type="number"/>
        <xsl:if test="position()=last()">
          <xsl:value-of select="substring-before(kml:coordinates,',')"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>
    <xsl:variable name="maxlat">
      <xsl:for-each select="msxsl:node-set($placemarks)/kml:Point">
        <xsl:sort select="substring-before(substring-after(kml:coordinates,','),',')" data-type="number"/>
        <xsl:if test="position()=last()">
          <xsl:value-of select="substring-before(substring-after(kml:coordinates,','),',')"/>
        </xsl:if>
      </xsl:for-each>
    </xsl:variable>

    <!-- Note the GPX element has @version=1.1 and @creator-->
    <gpx creator="Cybarber" version="1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1  http://www.topografix.com/GPX/1/1/gpx.xsd">
      <xsl:comment>xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"</xsl:comment>
      <xsl:comment>Transformation restrictions from KML version 2.0 to GPX version 1.1 are as follows:</xsl:comment>
      <xsl:comment>KML file structure needs to be: 'Document' with a 'Folder' named 'Waypoints' with waypoint 'Placemarks'</xsl:comment>
      <xsl:comment>a 'Folder' named 'Tracks' with sub-Folders for tracks</xsl:comment>
      <xsl:comment>each track subfolder contains a 'Placemark' named 'Path' with a track path and</xsl:comment>
      <xsl:comment>a subfolder named 'Points' with track point 'Placemark's.</xsl:comment>
      <xsl:comment>a 'Folder' named 'Routes' with sub-Folders for routes</xsl:comment>
      <xsl:comment>each route subfolder contains a 'Placemark' named 'Path' with a route path and</xsl:comment>
      <xsl:comment>a subfolder named 'Points' with route point 'Placemark's.</xsl:comment>
      <xsl:comment>by William A Slabbekoorn aka Cybarber,  September 2005.</xsl:comment>

      <metadata>
        <xsl:apply-templates select="kml:Document/kml:name|kml:Folder/kml:name"/>
        <!--
	<xsl:apply-templates select="kml:Document/kml:description|kml:Folder/kml:description"/>
-->
        <bounds minlat="{$minlat}" minlon="{$minlon}" maxlat="{$maxlat}" maxlon="{$maxlon}"/>
      </metadata>

      <xsl:apply-templates select="kml:Document//kml:Folder[kml:name ='Waypoints']/kml:Placemark[kml:Point]" mode="waypoints"/>
      <xsl:apply-templates select="kml:Document//kml:Folder[kml:name ='Routes']/kml:Folder" mode="routepoints"/>
      <xsl:apply-templates select="kml:Document//kml:Folder[kml:name ='Tracks']/kml:Folder" mode="trackpoints"/>

    </gpx>
  </xsl:template>


  <!-- Pulling the waypoints Folder Placemarks to WPT elements. -->
  <xsl:template match="kml:Placemark" mode="waypoints">
    <xsl:variable name="coords" select="kml:Point/kml:coordinates"/>
    <xsl:variable name="coordlon" select="substring-before($coords,',')"/>
    <xsl:variable name="coordrest" select="substring-after($coords,',')"/>
    <xsl:variable name="coordlat" select="substring-before($coordrest,',')"/>
    <wpt>
      <xsl:attribute name="lat">
        <xsl:value-of select="$coordlat"/>
      </xsl:attribute>
      <xsl:attribute name="lon">
        <xsl:value-of select="$coordlon"/>
      </xsl:attribute>
      <ele>
        <xsl:value-of select="substring-after($coordrest,',')"/>
      </ele>
      <xsl:apply-templates select="kml:name"/>
      <xsl:apply-templates select="kml:name" mode="cmt"/>
      <sym>
        <xsl:value-of select="'Waypoint'"/>
      </sym>
      <!-- later rework this somehow using the type of icon?-->
    </wpt>
  </xsl:template>

  <!-- Pulling the Route folders to RTE elements.  -->
  <xsl:template match="kml:Folder" mode="routepoints">
    <xsl:variable name="coords" select="kml:Placemark[kml:name = 'Path']//kml:coordinates"/>
    <xsl:choose>
      <xsl:when test="$processpath = 'dopath'">
        <rte>
          <xsl:apply-templates select="kml:name"/>
          <number>
            <xsl:value-of select="position()"/>
          </number>
          <xsl:call-template name="recursiveroutepath">
            <xsl:with-param name="coords" select="normalize-space($coords)"/>
          </xsl:call-template>
        </rte>
      </xsl:when>
      <xsl:when test="$processpoints = 'dopoints'">
        <rte>
          <xsl:apply-templates select="kml:name"/>
          <number>
            <xsl:value-of select="position()"/>
          </number>
          <xsl:apply-templates select="kml:Folder[kml:name = 'Points']/kml:Placemark[kml:Point]" mode="routepoints"/>
        </rte>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!-- 
	Utility template to recursively process the 
	route 'Path' placemark coordinates element 
    to create RTEPT elements
 -->
  <xsl:template name="recursiveroutepath">
    <xsl:param name="coords"/>
    <xsl:variable name="coordlon" select="substring-before($coords,',')"/>
    <xsl:variable name="coordrestlat" select="substring-after($coords,',')"/>
    <xsl:variable name="coordlat" select="substring-before($coordrestlat,',')"/>
    <xsl:variable name="coordrestele" select="substring-after($coordrestlat,',')"/>
    <xsl:variable name="coordele">
      <xsl:choose>
        <xsl:when test="contains($coordrestele,' ')">
          <xsl:value-of select="normalize-space(substring-before($coordrestele,' '))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($coordrestele)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="coordrest" select="substring-after($coordrestele,' ')"/>
    <rtept>
      <xsl:attribute name="lat">
        <xsl:value-of select="normalize-space($coordlat)"/>
      </xsl:attribute>
      <xsl:attribute name="lon">
        <xsl:value-of select="normalize-space($coordlon)"/>
      </xsl:attribute>
      <ele>
        <xsl:value-of select="$coordele"/>
      </ele>
    </rtept>
    <xsl:if test="contains($coordrest,',')">
      <xsl:call-template name="recursiveroutepath">
        <xsl:with-param name="coords" select="$coordrest"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Pulling the Routes Points folder Placemarks to RTEPT elements-->
  <xsl:template match="kml:Placemark" mode="routepoints">
    <xsl:variable name="coords" select="kml:Point/kml:coordinates"/>
    <xsl:variable name="coordlon" select="substring-before($coords,',')"/>
    <xsl:variable name="coordrest" select="substring-after($coords,',')"/>
    <xsl:variable name="coordlat" select="substring-before($coordrest,',')"/>
    <xsl:variable name="coordele" select="substring-after($coordrest,',')"/>
    <rtept>
      <xsl:attribute name="lat">
        <xsl:value-of select="$coordlat"/>
      </xsl:attribute>
      <xsl:attribute name="lon">
        <xsl:value-of select="$coordlon"/>
      </xsl:attribute>
      <ele>
        <xsl:value-of select="$coordele"/>
      </ele>
      <xsl:apply-templates select="kml:name"/>
      <xsl:apply-templates select="kml:name" mode="cmt"/>
    </rtept>
  </xsl:template>


  <!-- 
	Pulling the TRACK Folders to TRK/TRKSEG elements while processing either
	the POINTS placemarks to trkpt's or
	creating the trkpt's by recusing over 
	the 'coordinates' element of the PATH Placemark.
	Which ever is used is set by the processpath/processpoints PARAMETERS
	Default is PATH Processing.	
-->
  <xsl:template match="kml:Folder" mode="trackpoints">
    <xsl:variable name="coords" select="kml:Placemark[kml:name = 'Path']//kml:coordinates"/>
    <xsl:choose>
      <xsl:when test="$processpath = 'dopath'">
        <trk>
          <xsl:apply-templates select="kml:name"/>
          <number>
            <xsl:value-of select="position()"/>
          </number>
          <trkseg>
            <xsl:call-template name="recursivetrackpath">
              <xsl:with-param name="coords" select="normalize-space($coords)"/>
            </xsl:call-template>
          </trkseg>
        </trk>
      </xsl:when>
      <xsl:when test="$processpoints = 'dopoints'">
        <trk>
          <xsl:apply-templates select="kml:name"/>
          <number>
            <xsl:value-of select="position()"/>
          </number>
          <trkseg>
            <xsl:apply-templates select="kml:Folder[kml:name = 'Points']/kml:Placemark[kml:Point]" mode="trackpoints"/>
          </trkseg>
        </trk>
      </xsl:when>
    </xsl:choose>
  </xsl:template>
  <!-- 
	Utility template to recursively process the 
	track 'Path' placemark coordinates element 
    to create TRKPT elements
 -->
  <xsl:template name="recursivetrackpath">
    <xsl:param name="coords"/>
    <xsl:variable name="coordlon" select="substring-before($coords,',')"/>
    <xsl:variable name="coordrestlat" select="substring-after($coords,',')"/>
    <xsl:variable name="coordlat" select="substring-before($coordrestlat,',')"/>
    <xsl:variable name="coordrestele" select="substring-after($coordrestlat,',')"/>
    <xsl:variable name="coordele">
      <xsl:choose>
        <xsl:when test="contains($coordrestele,' ')">
          <xsl:value-of select="normalize-space(substring-before($coordrestele,' '))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($coordrestele)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>
    <xsl:variable name="coordrest" select="substring-after($coordrestele,' ')"/>
    <trkpt>
      <xsl:attribute name="lat">
        <xsl:value-of select="normalize-space($coordlat)"/>
      </xsl:attribute>
      <xsl:attribute name="lon">
        <xsl:value-of select="normalize-space($coordlon)"/>
      </xsl:attribute>
      <ele>
        <xsl:value-of select="$coordele"/>
      </ele>
    </trkpt>
    <xsl:if test="contains($coordrest,',')">
      <xsl:call-template name="recursivetrackpath">
        <xsl:with-param name="coords" select="$coordrest"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>

  <!-- Pulling the Track Points folder Placemarks to TRKPT elements [(position() mod 3)= 0]-->
  <xsl:template match="kml:Placemark" mode="trackpoints">
    <xsl:if test="position() = 1 or (position() mod $multi500) = 0">
      <xsl:variable name="coords" select="normalize-space(kml:Point/kml:coordinates)"/>
      <xsl:variable name="coordlon" select="substring-before($coords,',')"/>
      <xsl:variable name="coordrest" select="substring-after($coords,',')"/>
      <xsl:variable name="coordlat" select="substring-before($coordrest,',')"/>
      <xsl:variable name="coordele" select="substring-after($coordrest,',')"/>
      <trkpt>
        <xsl:attribute name="lat">
          <xsl:value-of select="$coordlat"/>
        </xsl:attribute>
        <xsl:attribute name="lon">
          <xsl:value-of select="$coordlon"/>
        </xsl:attribute>
        <ele>
          <xsl:value-of select="$coordele"/>
        </ele>
        <xsl:apply-templates select="kml:name"/>
        <xsl:apply-templates select="kml:name" mode="cmt"/>
      </trkpt>
    </xsl:if>
  </xsl:template>

  <xsl:template match="kml:name">
    <name>
      <xsl:value-of select="."/>
    </name>
  </xsl:template>

  <xsl:template match="kml:name" mode="cmt">
    <cmt>
      <xsl:value-of select="."/>
    </cmt>
  </xsl:template>

  <xsl:template match="kml:description" mode="path">
    <name>
      <xsl:value-of select="."/>
    </name>
  </xsl:template>

  <xsl:template match="kml:description">
    <desc>
      <xsl:value-of select="."/>
    </desc>
  </xsl:template>

  <xsl:template match="kml:Snippet"/>
  <xsl:template match="kml:visibility"/>
  <xsl:template match="kml:open"/>
  <xsl:template match="kml:tessellate"/>
  <xsl:template match="kml:extrude"/>
  <xsl:template match="kml:altitudeMode"/>
  <xsl:template match="kml:address"/>
  <xsl:template match="kml:coordinates">
    <xsl:value-of select="."/>
  </xsl:template>
  <xsl:template match="kml:LineString"/>
  <xsl:template match="kml:Point"/>
  <!-- MultiGeometry with Polygons NOT processed to GPX format -->
  <xsl:template match="kml:GeometryCollection"/>
  <xsl:template match="kml:MultiGeometry"/>
  <xsl:template match="kml:Polygon"/>
  <xsl:template match="kml:outerBoundaryIs"/>
  <xsl:template match="kml:innerBoundaryIs"/>
  <xsl:template match="kml:LinearRing"/>
  <xsl:template match="kml:LookAt"/>
  <xsl:template match="kml:longitude"/>
  <xsl:template match="kml:latitude"/>
  <xsl:template match="kml:range"/>
  <xsl:template match="kml:tilt"/>
  <xsl:template match="kml:heading"/>
  <!-- timeposition Processed rest ignored-->
  <xsl:template match="kml:TimePeriod"/>
  <xsl:template match="kml:begin"/>
  <xsl:template match="kml:end"/>
  <xsl:template match="kml:TimeInstance">
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="kml:timePosition">
    <time>
      <xsl:value-of select="."/>
    </time>
  </xsl:template>
  <!-- Style elements NOT processed to GPX format -->
  <xsl:template match="kml:styleUrl"/>
  <xsl:template match="kml:styleBlinker"/>
  <xsl:template match="kml:StyleMap"/>
  <xsl:template match="kml:Pair"/>
  <xsl:template match="kml:key"/>
  <xsl:template match="kml:Style"/>
  <xsl:template match="kml:color"/>
  <xsl:template match="kml:scale"/>
  <xsl:template match="kml:polyMode"/>
  <xsl:template match="kml:geomScale"/>
  <xsl:template match="kml:geomColor"/>
  <xsl:template match="kml:labelScale"/>
  <xsl:template match="kml:labelColor"/>
  <xsl:template match="kml:LineStyle"/>
  <xsl:template match="kml:PolyStyle"/>
  <xsl:template match="kml:outline"/>
  <xsl:template match="kml:fill"/>
  <xsl:template match="kml:IconStyle"/>
  <xsl:template match="kml:Icon"/>
  <xsl:template match="kml:href"/>
  <xsl:template match="kml:w"/>
  <xsl:template match="kml:h"/>
  <xsl:template match="kml:x"/>
  <xsl:template match="kml:y"/>
  <!-- GroundOverlays NOT processed to GPX format -->
  <xsl:template match="kml:GroundOverlay"/>
  <xsl:template match="kml:LatLonBox"/>
  <xsl:template match="kml:rotation"/>
  <xsl:template match="kml:west"/>
  <xsl:template match="kml:east"/>
  <xsl:template match="kml:south"/>
  <xsl:template match="kml:north"/>
  <!-- ScreenOverlays NOT processed to GPX format -->
  <xsl:template match="kml:ScreenOverlay"/>
  <xsl:template match="kml:overlayXY"/>
  <xsl:template match="kml:screenXY"/>
  <xsl:template match="kml:size"/>
  <xsl:template match="kml:drawOrder"/>
  <!-- NetworkLinks NOT processed to GPX format -->
  <xsl:template match="kml:NetworkLink"/>
  <xsl:template match="kml:Url"/>
  <xsl:template match="kml:refreshVisibility"/>
  <xsl:template match="kml:refreshInterval"/>
  <xsl:template match="kml:refreshMode"/>
  <xsl:template match="kml:viewRefreshMode"/>
  <xsl:template match="kml:viewRefreshTime"/>
</xsl:stylesheet>