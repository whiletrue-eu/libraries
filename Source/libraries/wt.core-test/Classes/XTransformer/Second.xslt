<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
    <xsl:output method="xml"/>

    <xsl:template match="/">
      <Data Include="{document('include.xml')/*/@Include}">
        <xsl:copy-of select="."/>
      </Data>
    </xsl:template>
</xsl:stylesheet>
