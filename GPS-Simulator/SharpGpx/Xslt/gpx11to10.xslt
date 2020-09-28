<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  xmlns="http://www.topografix.com/GPX/1/0" xmlns:gpx="http://www.topografix.com/GPX/1/1" exclude-result-prefixes="gpx">

  <xsl:output method="xml" indent="yes" encoding="utf-8"/>

  <xsl:template match="/gpx:gpx">
    <gpx creator="Cybarber" version="1.0" xmlns:xsd="http://www.w3.org/2001/XMLSchema"  xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  xsi:schemaLocation="http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd">
      <xsl:comment>xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"  xsi:schemaLocation="http://www.topografix.com/GPX/1/0 http://www.topografix.com/GPX/1/0/gpx.xsd"</xsl:comment>
      <xsl:comment>Transformation changes from GPX version 1.1 to version 1.0 are as follows:</xsl:comment>
      <xsl:comment>For all occurances: Link element re-written in url and urlname elements,  </xsl:comment>
      <xsl:comment>extensions elelement disregarded, child element type of trk and rte elements disregarded, </xsl:comment>
      <xsl:comment>trk child elements course and speed not created as they are ovbsolete in version 1.1</xsl:comment>
      <xsl:comment>metadata deleted but contents processed: author and email re-written, copyright element disregarded.</xsl:comment>
      <xsl:comment>by William A Slabbekoorn aka Cybarber,  August 2005.</xsl:comment>
      <xsl:apply-templates/>
    </gpx>
  </xsl:template>
  <xsl:template match="gpx:metadata">
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="gpx:author">
    <author>
      <xsl:value-of select="gpx:name"/>
    </author>
    <xsl:apply-templates/>
  </xsl:template>
  <xsl:template match="gpx:email">
    <email>
      <xsl:value-of select="@id"/>
      <xsl:text>@</xsl:text>
      <xsl:value-of select="@domain"/>
    </email>
  </xsl:template>
  <xsl:template match="gpx:link">
    <url>
      <xsl:value-of select="@href"/>
    </url>
    <urlname>
      <xsl:value-of select="gpx:text"/>
    </urlname>
  </xsl:template>
  <xsl:template match="gpx:*">
    <xsl:element name="{local-name()}">
      <xsl:copy-of select="@*"/>
      <xsl:apply-templates/>
    </xsl:element>
  </xsl:template>
  <xsl:template match="gpx:author/gpx:name"/>
  <xsl:template match="gpx:author/gpx:link"/>
  <xsl:template match="gpx:copyright"/>
  <xsl:template match="gpx:extensions"/>
  <xsl:template match="gpx:trk/gpx:type | gpx:rte/gpx:type"/>
  <!--

    __   Script: William A Slabbekoorn - Aug 2005
   / _)      _        __   _    _  __ 
   |(_ \\ /||_) //_\ ||_)||_) ||_ ||_)
   \__) || ||__)|| | || \||__)||_ || \
	 
	This is a MSXML3/4/5 XSLT transformation application which 
	converts GPX version 1.1 format to GPX version 1.0 format.
	  
	Mid 2005 many applications using GPX could only use version 1.0
	while  more and more applications only could produce GPX version 1.1.
	
	A version 1.0 to GPX 1.1 is also available:
	http://members.home.nl/cybarber/geomatters/gpx10to11.xslt
	aswell as a GPX to Goolge Earth KML 2.0 file format. 	
	
	Copyright: Cybarber Web Services, William A Slabbekoorn, August 2005.
	http://members.home.nl/cybarber/geomatters/gpx11to10.xslt
							 
				 
	-->

</xsl:stylesheet>