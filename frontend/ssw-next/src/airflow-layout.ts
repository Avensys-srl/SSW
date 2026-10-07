// Positions are the numbered connections in the manufacturer's layout sheet.
// CSS slots describe the view, not the identity of a connection.
export const airflowSlot = (position: number, sameSide: boolean, eastWest: boolean): number =>
  sameSide || eastWest ? position : position <= 2 ? position + 2 : position - 2;

export const airflowSide = (position: number, sameSide: boolean, flatFloor: boolean, eastWest: boolean): string =>
  sameSide ? flatFloor ? "south" : "north" : eastWest
    ? position <= 2 ? "west" : "east"
    : position <= 2 ? "south" : "north";
