/* ******************************************************************************************************** */
/* This file has been augmented by Fyodor Soikin, after downloading from Microsoft.
/* History:
/*      2012-12-12:     Added "dialog" to the JQuery interface
/*      2012-12-14:     Added "position" to the JQuery interface
/*      2013-02-16:     Changed "DialogOptions.width" from "number" to "any"
/*      2013-02-19:     Added "DialogOptions.beforeClose"
/*      2013-02-26:     Added "JQuery.button"
/* ******************************************************************************************************** */

/// <reference path="jquery.d.ts"/>

// Partial typing for the jQueryUI library, version 1.8.x

interface DraggableEventUIParam {
    helper: JQuery;
    position: { top: number; left: number;};
    offset: { top: number; left: number;};
}

interface DraggableEvent {
    (event: Event, ui: DraggableEventUIParam): void;
}

interface Draggable {
    // Options
    disabled?: boolean;
    addClasses?: boolean;
    appendTo?: any;
    axis?: string;
    cancel?: string;
    connectToSortable?: string;
    containment?: any;
    cursor?: string;
    cursorAt?: any;
    delay?: number;
    distance?: number;
    grid?: number[];
    handle?: any;
    helper?: any;
    iframeFix?: any;
    opacity?: number;
    refreshPositions?: boolean;
    revert?: any;
    revertDuration?: number;
    scope?: string;
    scroll?: boolean;
    scrollSensitivity?: number;
    scrollSpeed?: number;
    snap?: any;
    snapMode?: string;
    snapTolerance?: number;
    stack?: string;
    zIndex?: number;
    // Events
    create?: DraggableEvent;
    start?: DraggableEvent;
    drag?:  DraggableEvent;
    stop?:  DraggableEvent;
}

interface DroppableEventUIParam {
    draggable: JQuery;
    helper: JQuery;
    position: { top: number; left: number;};
    offset: { top: number; left: number;};
}

interface DroppableEvent {
    (event: Event, ui: DroppableEventUIParam): void;
}

interface Droppable {
    // Options
    disabled?: boolean;
    accept?: any;
    activeClass?: string;
    greedy?: boolean;
    hoverClass?: string;
    scope?: string;
    tolerance?: string;
    // Events
    create?: DroppableEvent;
    activate?: DroppableEvent;
    deactivate?: DroppableEvent;
    over?: DroppableEvent;
    out?: DroppableEvent;
    drop?: DroppableEvent;
}

interface DialogOptions {
    title?: string;
    width?: any; /* number or 'auto' */
    height?: number;
    buttons?: any;
    beforeClose?: (event: JQueryEventObject, ui: JQuery) => void;
}

interface JQueryPosition
{ my: string; at: string; of: any; offset?: string; }

interface JQuery {
    draggable(options: Draggable): JQuery;
    draggable(optionLiteral: string, options: Draggable): JQuery;
    draggable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    draggable(optionLiteral: string, optionName: string): any;
    // draggable(methodName: string): any;
    droppable(options: Droppable): JQuery;
    droppable(optionLiteral: string, options: Draggable): JQuery;
    droppable(optionLiteral: string, optionName: string, optionValue: any): JQuery;
    droppable(optionLiteral: string, optionName: string): any;
    droppable(methodName: string): any;

    dialog(command:string, arg?: any): JQuery;
    dialog(options:DialogOptions): JQuery;

    position(options: JQueryPosition): JQuery;

    button(options?: any): JQuery;
    button(command: string, arg?: any): JQuery;
}