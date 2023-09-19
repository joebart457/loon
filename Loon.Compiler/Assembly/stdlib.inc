proc strlen stdcall strInput:dword
	mov eax, [strInput]
	test eax, eax
	jz FAIL_NULL_PTR
	xor eax, eax
	mov ecx, -1
	mov al, 0
	mov edi, [strInput]
	cld
	repne scasb
	not ecx
	dec ecx
	mov eax, ecx
	ret
endp


proc strcpy stdcall dest:dword, source:dword
	stdcall strlen, dword [source]
	mov ecx, eax
	mov edi, [dest]
	mov esi, [source]
	rep movsb
	ret
endp

proc strcmp stdcall lhs:dword, rhs:dword
	
	local lhsLen:DWORD

	mov eax, dword [lhs]
	test eax, eax
	jz FAIL_NULL_PTR
	mov eax, dword [rhs]
	test eax, eax
	jz FAIL_NULL_PTR
	
	stdcall strlen, dword [lhs]
	mov [lhsLen], eax
	stdcall strlen, dword [rhs]

	mov ebx, dword [lhsLen]
	cmp eax, ebx
	jne __STRCMP_NOT_EQUAL

	cld
	mov ecx, dword [lhsLen]
	mov edi, dword [lhs]
	mov esi, dword [rhs]
	repe cmpsb	
	jne __STRCMP_NOT_EQUAL

	mov eax, 0
	jmp __STRCMP_END

	__STRCMP_NOT_EQUAL:
	mov eax, 1
	__STRCMP_END:
	ret
endp

proc strdup stdcall src:dword
	local result:DWORD

	stdcall strlen, dword [src]
	stdcall AllocHeap, eax
	mov dword [result], eax
	stdcall strcpy, dword [result], dword [src]
	ret
endp


proc strcat stdcall lhs:dword, rhs:dword
	local lhsLen:DWORD
	local rhsLen:DWORD
	local result:DWORD

	stdcall strlen, dword [lhs]
	mov [lhsLen], eax
	stdcall strlen, dword [rhs]
	mov [rhsLen], eax
	add eax, dword [lhsLen]
	inc eax
	stdcall AllocateMemory, eax
	mov dword [result], eax
	cld
	xor ecx, ecx
	mov edi, dword [result]
	mov esi,  dword [lhs]
	.copy_byte_lhs:
		lodsb
		cmp al, byte 0 ; assumes zero terminated string
		je .copy_byte_rhs
		mov byte [edi], al
		inc edi
		jmp .copy_byte_lhs
	.copy_byte_rhs:
		mov esi, dword [rhs]
	.copy_byte:
		lodsb
		cmp al, 0 ; assumes zero terminated string
		je .end_copy
		mov byte [edi], al
		inc edi
		jmp .copy_byte
	.end_copy:
		mov eax, dword [result]
	ret

endp


proc AllocateMemory stdcall nSize:DWORD	
	mov eax, dword [nSize]
	invoke HeapAlloc, [_hHeap], dword HEAP_ZERO_MEMORY, eax
	test eax, eax
	jz FAIL_ALLOC
	ret
endp

proc FreeHeap stdcall oPtr:DWORD
	mov eax, [oPtr]
	test eax, eax
	jmp .end_free
	invoke HeapFree, dword [_hHeap], dword 0, dword [oPtr]
	test eax, eax
	jz FAIL_HEAP_FREE
	.end_free:
		ret
endp

proc CopyMemory stdcall uses eax,ebx,ecx,edx, dest:dword, src:dword, nBytes:dword

	mov edx, dword [dest]
	test edx, edx
	jz FAIL_NULL_PTR
	mov ebx, dword [src]
	test ebx, ebx
	jz FAIL_NULL_PTR
	mov ecx, dword [nBytes]
	cmp ecx, dword 0
	ja FAIL_ALLOC
	.loop_start:
	test ecx, ecx
	jz .loop_end
	mov al, byte [edx]
	mov byte [src], al
	dec ecx
	inc dword [edx]
	inc dword [src]
	jmp .loop_start
	.loop_end:
	xor edx, edx
	ret
endp