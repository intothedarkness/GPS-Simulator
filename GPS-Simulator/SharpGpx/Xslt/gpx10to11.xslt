<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsd="http://www.w3.org/2001/XMLSchema"  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="http://www.topografix.com/GPX/1/1" xmlns:gpx="http://www.topografix.com/GPX/1/0" exclude-result-prefixes="gpx">

  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:template match="/gpx:gpx">
    <gpx creator="Cybarber" version="1.1" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd">
      <xsl:comment>xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:schemaLocation="http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd"</xsl:comment>
      <xsl:comment>Transformation changes from GPX version 1.0 to version 1.1 are as follows:</xsl:comment>
      <xsl:comment>All gpx element child elements except wpt, rte and trk  regrouped under newly created metadata element</xsl:comment>
      <xsl:comment>author and email re-written, author child link element and copyright elements not created. </xsl:comment>
      <xsl:comment>all occurances of url and urlname elements re-written as Link element,  </xsl:comment>
      <xsl:comment>extensions element,  trk and rte child elements not created.</xsl:comment>
      <xsl:comment>trk child elements course and speed disregarded as they are obsolete in version 1.1</xsl:comment>
      <xsl:comment>by William A Slabbekoorn aka Cybarber,  August 2005.</xsl:comment>
      <metadata>
        <xsl:apply-templates select="./*[not(local-name() = 'wpt' or local-name() = 'rte' or local-name() = 'trk')]"/>
      </metadata>
      <xsl:apply-templates select="gpx:wpt | gpx:rte | gpx:trk"/>
    </gpx>
  </xsl:template>
  <xsl:template match="gpx:author">
    <author>
      <name>
        <xsl:value-of select="."/>
      </name>
      <xsl:apply-templates select="../gpx:email" mode="author"/>
    </author>
  </xsl:template>

  <xsl:template match="gpx:email" mode="author">
    <email id="{substring-before(. , '@')}" domain="{substring-after(. , '@')}"/>
  </xsl:template>

  <xsl:template match="gpx:email" />

  <xsl:template match="gpx:url">
    <link href="{.}">
      <text>
        <xsl:value-of select="../gpx:urlname"/>
      </text>
    </link>
  </xsl:template>
  <xsl:template match="gpx:urlname"/>
  <!-- empty -->
  <xsl:template match="gpx:trk/gpx:course"/>
  <xsl:template match="gpx:trk/gpx:speed"/>
  <xsl:template match="gpx:*">
    <xsl:element name="{local-name()}">
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  <!--

    __   Script: William A Slabbekoorn - Aug 2005
   / _)      _        __   _    _  __ 
   |(_ \\ /||_) //_\ ||_)||_) ||_ ||_)
   \__) || ||__)|| | || \||__)||_ || \
	 
	This is a MSXML3/4/5 XSLT transformation application which 
	converts GPX version 1.0 format to GPX version 1.1 format.
	  
	Mid 2005 many applications using GPX could only produce version 1.0
	while  more and more applications only could use GPX version 1.1.
	
	A version 1.1 to GPX 1.0 is also available:
	http://members.home.nl/cybarber/geomatters/gpx11to10.xslt
	aswell as a GPX to Goolge Earth KML 2.0 file format. 	
	
	Copyright: Cybarber Web Services, William A Slabbekoorn, August 2005.
	http://members.home.nl/cybarber/geomatters/gpx10to11.xslt
							 
-->
</xsl:stylesheet>