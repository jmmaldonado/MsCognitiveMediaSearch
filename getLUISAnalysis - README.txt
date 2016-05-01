getLUISAnalysis
===============

Create an azure function with this name - or any other for that matter - and invoke it via POST.
The POST body must contanin the query you want to process in plain text, not json, not xml, just plain query.
It does not require authentication

If using postman to test the function select RAW for the body data type.


Requires the following Application Settings in the Application Service Plan
===============		====================================================
AppSetting Name		Sample Value
===============		====================================================
LUISMaxCharacters	400
LUISBaseURI			https://api.projectoxford.ai/luis/v1/application?id=
LUISAppID			ffffffffff-ffffffff-fffff-fffffff-ffffff
LUISKey				00478a767b7bbd77236c