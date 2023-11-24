export interface Pagination {
  currentPage: number;
  itemsPerPage: number;
  totalItems: number;
  totalPages: number;
}

export class PaginatedResult<T> {
  result?: T; // Set this to the list of results which are returned
  pagination?: Pagination; // Set this to pagination info from the http header
}
