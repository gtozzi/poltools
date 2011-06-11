/*
        Maintened by *Edwards
        For FantasiaShard.com

        Edwards@FantasiaShard.com

        2009-10-20
        
        Last Modifications 2009-10-28
             * Added CP_ListRootItemsInContainer(container)
        
        Author: Austin
        
        Purpose
        Assists in the use of storage areas.
        
        CP_GetStorageContainerForMobile(container_id, storage_area_name:=0)
        CP_GetStorageAreaContainer(mobile, storage_area_name:=0, create:=CP_NOCREATE)
        CP_CreateStorageAreaContainer(container_id, storage_area_name:=0)
        CP_RegisterStorageArea(storage_area_name)
        CP_RemoveStorageAreaContainer(container_id, storage_area_name)
        CP_StorageDebugMsg(text)
        CP_ListRootItemsInContainer(container)
*/
use uo;
use os;
use file;
use polsys;
use storage;

// * Global Variables *
CONST CP_NOCREATE	:= 0x0;
CONST CP_CREATE		:= 0x1;

CONST CP_DEFAULT_AREA	:= "World Bank";
CONST CP_DEFAULT_PREFIX	:= "[Root Container] ";

var container_type	:= "BankBox";

// Toggles the bank related debug messages on and off.
var STORAGE_DEBUG	:= 1;

function CP_GetMobileContainerID( mobile )

	if( mobile.IsA( POLCLASS_CORPSE ))
		return "Owner="+Hex( mobile.ownerserial );
	else
		return "Owner="+Hex( mobile.serial );
	endif
endfunction

function CP_GetStorageContainerForMobile( mobile, storage_area_name:=0, create:=CP_NOCREATE )

	if(( !storage_area_name ) || ( storage_area_name == error ))
		storage_area_name := CP_DEFAULT_AREA;
	endif

	var container_id := CP_GetMobileContainerID( mobile );
	var root_container := CP_GetStorageAreaContainer( container_id, storage_area_name, create );

	return root_container;
endfunction

function CP_GetStorageAreaContainer( container_id, storage_area_name:=0, create:=CP_NOCREATE )

	if(( !storage_area_name ) || ( storage_area_name == error ))
		storage_area_name := CP_DEFAULT_AREA;
	endif
 
	var container_name := CP_DEFAULT_PREFIX + container_id;

	var storage_area := FindStorageArea( storage_area_name );
	if( !storage_area )
		if( !create )
			return storage_area; // Will be an error
		else
			return CP_CreateStorageAreaContainer( container_id, storage_area_name );
		endif
	endif

	var root_container := FindRootItemInStorageArea( storage_area, container_name );
	if( !root_container && create )
		return CP_CreateStorageAreaContainer( container_id, storage_area_name );
	else
		return root_container;
	endif
endfunction

function CP_CreateStorageAreaContainer( container_id, storage_area_name:=0 )

	if(( !storage_area_name ) || ( storage_area_name == error ))
		storage_area_name := CP_DEFAULT_AREA;
	endif

	var storage_area := FindStorageArea( storage_area_name );
	if( !storage_area )
		// The storage area does not exist - so create it.
		storage_area := CreateStorageArea( storage_area_name );
	endif

	var container_name := CP_DEFAULT_PREFIX + container_id;

	var root_container := CreateRootItemInStorageArea( storage_area, container_name, container_type );
	if( root_container.errortext )
		CP_StorageDebugMsg( "Error::CP_CreateStorageAreaContainer() - "+root_container.errortext );
	endif

	return root_container;
endfunction

function CP_RemoveStorageAreaContainer( container_id, storage_area_name:=0 )

	if(( !storage_area_name ) || ( storage_area_name == error ))
		storage_area_name := CP_DEFAULT_AREA;
	endif

	var storage_area := FindStorageArea( storage_area_name );
	var container_name := CP_DEFAULT_PREFIX + container_id;

	return DestroyRootItemInStorageArea( storage_area, container_name );
endfunction

function CP_GetStorageAreaNames()

	var name_list := array;
	foreach storage_area in ( StorageAreas() )
		// Used to fix unused var report by ecompile: storage_area;
		storage_area := storage_area;
		name_list.Append( _storage_area_iter );
		SleepMS(2);
	endforeach

	return name_list;
endfunction

function CP_GetRootContainersInStorageArea( storage_area_name )

	var storage := StorageAreas();
	var root_list := array;
	foreach container in ( storage[storage_area_name] )
		root_list.Append( container );
		SleepMS(2);
	endforeach

	return root_list;
endfunction

function CP_StorageDebugMsg( report_text )

	if( STORAGE_DEBUG )
		var script_name := GetProcess( GetPid() ).name;
		LogToFile( "::log/storage.log", "["+script_name+"]: "+report_text, LOG_DATETIME );
	endif

	return 1;
endfunction

function CP_ListRootItemsInContainer( container )

	var root_items := array{};
        foreach item in EnumerateItemsInContainer( container )
		if( item.container == container )
			root_items.Append( item );
		endif
		SleepMS(2);
	endforeach

	return root_items;
endfunction