<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
                xmlns:stylesheets="ext:stylesheets"
                xmlns:xmlutils="ext:xml"
>
    <xsl:output method="xml"/>

    <xsl:template match="/">
      <xsl:value-of select="stylesheets:load('Second','Second.xslt')"/>
      <xsl:copy-of select="xmlutils:toXmlFragment(stylesheets:transform('Second',.))"/>
    </xsl:template>
</xsl:stylesheet>
