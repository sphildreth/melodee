#!/usr/bin/python3
import os
import shutil
import re
import argparse
import zipfile
from collections import defaultdict
import time
import math
import signal
import sys
import binascii
import zlib
from typing import Optional, Tuple, Dict, List

# Import tqdm for progress indication
try:
    from tqdm import tqdm
    TQDM_AVAILABLE = True
except ImportError:
    TQDM_AVAILABLE = False
    print("Warning: tqdm not available. Install with 'pip install tqdm' for progress bars.")

# Import PIL for image processing
try:
    from PIL import Image
    PIL_AVAILABLE = True
except ImportError:
    PIL_AVAILABLE = False
    print("Warning: PIL (Pillow) not available. Install with 'pip install Pillow' for image processing.")

print("DEBUG: Script loaded successfully")

# List of keywords to search for in directory names (case-insensitive)
KEYWORDS = ["Live", "VA", "Greatest", "Hits", "Show", "Radio", "Single", "Billboard", "Top", "Charts", "Compilation", "Collection", "Best Of", "DJ Mix", "Live Mix"]

# Pre-compiled regex patterns for better performance
DATE_PATTERN_1 = re.compile(r'\b\d{2}-\d{2}-\d{4}\b|\b\d{4}-\d{2}-\d{2}\b|\b\d{2}-\d{2}-\d{2}\b', re.IGNORECASE)
WEEKDAYS = r'(MON|TUE|WED|THU|FRI|SAT|SUN)'
DATE_PATTERN_2 = re.compile(rf'\b\d{{2}}-\d{{2}}-{WEEKDAYS}\b', re.IGNORECASE)
DATE_PATTERN_3 = re.compile(rf'\b{WEEKDAYS}-\d{{2}}-\d{{2}}\b', re.IGNORECASE)
COMBINED_DATE_PATTERN = re.compile(rf'({DATE_PATTERN_1.pattern})|({DATE_PATTERN_2.pattern})|({DATE_PATTERN_3.pattern})', re.IGNORECASE)
START_DATE_PATTERN = re.compile(rf'^({DATE_PATTERN_1.pattern}|{DATE_PATTERN_2.pattern}|{DATE_PATTERN_3.pattern})\/', re.IGNORECASE)

# Pre-compiled keyword patterns
KEYWORD_PATTERNS = {
    keyword: re.compile(rf'(?<![a-zA-Z0-9]){re.escape(keyword)}(?![a-zA-Z0-9])', re.IGNORECASE) 
    for keyword in KEYWORDS
}

# Image file extensions to check
IMAGE_EXTENSIONS = {'.jpg', '.jpeg', '.png', '.gif', '.bmp', '.tiff', '.webp'}

# Pre-compiled pattern for "proof" in filenames
PROOF_PATTERN = re.compile(r'proof', re.IGNORECASE)

# Pre-compiled pattern for SFV files
SFV_PATTERN = re.compile(r'\.sfv$', re.IGNORECASE)

# SFV file extensions to check
SFV_EXTENSIONS = {'.sfv'}

# Global flag for graceful shutdown
shutdown_requested = False

# ANSI color codes for modern output
class Colors:
    RED = '\033[91m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    MAGENTA = '\033[95m'
    CYAN = '\033[96m'
    WHITE = '\033[97m'
    BOLD = '\033[1m'
    UNDERLINE = '\033[4m'
    RESET = '\033[0m'
    
    # Background colors
    BG_RED = '\033[101m'
    BG_GREEN = '\033[102m'
    BG_BLUE = '\033[104m'
    BG_YELLOW = '\033[103m'

# Statistics tracking
stats = defaultdict(int)
start_time = time.time()

def unzip_files_in_directory(dir_path, pretend=True):
    """Unzip any ZIP files found in the directory."""
    zip_files = []
    for item in os.listdir(dir_path):
        if item.lower().endswith('.zip'):
            zip_files.append(os.path.join(dir_path, item))
    
    for zip_file in zip_files:
        try:
            if pretend:
                print(f"  {Colors.YELLOW}📦 Would unzip:{Colors.RESET} {zip_file}")
            else:
                print(f"  {Colors.GREEN}📦 Unzipping:{Colors.RESET} {zip_file}")
                with zipfile.ZipFile(zip_file, 'r') as zip_ref:
                    zip_ref.extractall(dir_path)
                os.remove(zip_file)
            stats['zip_files_processed'] += 1
        except Exception as e:
            print(f"  {Colors.RED}❌ Error processing {zip_file}: {e}{Colors.RESET}")

def is_directory_empty(dir_path):
    """Check if directory is empty (no files or subdirectories)."""
    try:
        return len(os.listdir(dir_path)) == 0
    except OSError:
        return False

def format_size(size_bytes):
    """Convert bytes to human readable format."""
    if size_bytes == 0:
        return "0 B"
    size_names = ["B", "KB", "MB", "GB", "TB"]
    i = int(math.floor(math.log(size_bytes, 1024)))
    p = math.pow(1024, i)
    s = round(size_bytes / p, 2)
    return f"{s} {size_names[i]}"

def get_directory_size(dir_path, chunk_size=1000):
    """
    Calculate total size of directory with memory-efficient chunked processing.
    
    Args:
        dir_path: Path to directory
        chunk_size: Number of files to process in each chunk
    
    Returns:
        Total size in bytes
    """
    global shutdown_requested
    total_size = 0
    file_count = 0
    
    try:
        for dirpath, dirnames, filenames in os.walk(dir_path):
            if shutdown_requested:
                break
                
            # Process files in chunks to manage memory usage
            for i in range(0, len(filenames), chunk_size):
                if shutdown_requested:
                    break
                    
                chunk = filenames[i:i + chunk_size]
                for filename in chunk:
                    filepath = os.path.join(dirpath, filename)
                    try:
                        total_size += os.path.getsize(filepath)
                        file_count += 1
                    except (OSError, FileNotFoundError):
                        pass
                
                # Small delay every chunk to allow for interruption
                if file_count % (chunk_size * 10) == 0:
                    time.sleep(0.001)
                    
    except (OSError, FileNotFoundError):
        pass
    return total_size

def should_delete(dirname, delete_dash_one=True):
    """Check if any of the keywords are in the directory name, if it contains a date, or ends with '-1'."""
    keyword_match = any(pattern.search(dirname) for pattern in KEYWORD_PATTERNS.values())
    dash_one_match = delete_dash_one and dirname.strip().endswith('-1')
    return keyword_match or contains_embedded_date(dirname) or dash_one_match

def highlight_deletion_reason(dirname, delete_dash_one=True):
    """
    Highlight the reason why a directory would be deleted.
    Returns the directory name with highlighted keywords/dates and reason.
    """
    highlighted_name = dirname
    reasons = []
    # Check for keywords and highlight them using pre-compiled patterns
    for keyword, pattern in KEYWORD_PATTERNS.items():
        if pattern.search(dirname):
            highlighted_name = pattern.sub(f"{Colors.BG_RED}{Colors.WHITE}{keyword.upper()}{Colors.RESET}", highlighted_name)
            reasons.append(f"keyword '{Colors.BOLD}{keyword}{Colors.RESET}'")
    # Check for embedded dates and highlight them
    if contains_embedded_date(dirname):
        matches = list(COMBINED_DATE_PATTERN.finditer(dirname))
        if matches:
            for match in reversed(matches):
                date_text = match.group()
                highlighted_date = f"{Colors.BG_YELLOW}{Colors.BOLD}{date_text}{Colors.RESET}"
                highlighted_name = highlighted_name[:match.start()] + highlighted_date + highlighted_name[match.end():]
            reasons.append(f"embedded date pattern")
    # Check for '-1' suffix
    if delete_dash_one and dirname.strip().endswith('-1'):
        highlighted_name = re.sub(r'(-1)$', f"{Colors.BG_RED}{Colors.WHITE}-1{Colors.RESET}", highlighted_name)
        reasons.append(f"ends with '-1' (likely duplicate)")
    if reasons:
        reason_str = f" {Colors.CYAN}[Reason: {', '.join(reasons)}]{Colors.RESET}"
    else:
        reason_str = ""
    return highlighted_name + reason_str

def contains_embedded_date(s: str) -> bool:
    """
    Returns True if the string contains a date in any of the following formats,
    but only if the string contains other text besides the date itself.
    Returns False if the string starts with a date followed by a '/' (e.g., '2025-07-10/...').
    Returns True if the string starts with a date + '/' but the rest ALSO contains a date.
    """
    # Check for date at the very start followed by '/' using pre-compiled pattern
    if START_DATE_PATTERN.match(s):
        # If there is another date after the '/', return True; else False
        rest = s.split('/', 1)[1]
        if COMBINED_DATE_PATTERN.search(rest):
            # Check that it's embedded, not just another date alone
            matches = list(COMBINED_DATE_PATTERN.finditer(rest))
            for match in matches:
                before = rest[:match.start()]
                after = rest[match.end():]
                if before.strip() or after.strip():
                    return True
            return False
        return False

    # Main logic for other cases using pre-compiled pattern
    matches = list(COMBINED_DATE_PATTERN.finditer(s))
    if not matches:
        return False
    for match in matches:
        before = s[:match.start()]
        after = s[match.end():]
        if before.strip() or after.strip():
            return True
    return False

def delete_matching_dirs(root_dir, pretend=True, check_sfv=True, delete_dash_one=True):
    """Recursively process directories under root_dir with optimized single-pass processing."""
    global shutdown_requested
    deleted = 0
    
    print(f"\n{Colors.CYAN}🔍 Scanning directories...{Colors.RESET}")
    
    # First, count total directories for progress indication
    total_dirs = 0
    total_files = 0
    if TQDM_AVAILABLE:
        print(f"{Colors.BLUE}📊 Counting items for progress tracking...{Colors.RESET}")
        for dirpath, dirnames, filenames in os.walk(root_dir):
            if shutdown_requested:
                return deleted
            total_dirs += len(dirnames)
            total_files += len(filenames)
    
    # Single-pass processing with progress indication
    processed_dirs = 0
    directories_to_process = []
    
    # Collect all directories and files in a single walk
    print(f"\n{Colors.MAGENTA}🔄 Processing directories and files...{Colors.RESET}")
    
    # Use progress bar if available
    pbar = None
    if TQDM_AVAILABLE and total_dirs > 0:
        pbar = tqdm(total=total_dirs, desc="Processing directories", 
                   bar_format='{desc}: {percentage:3.0f}%|{bar}| {n_fmt}/{total_fmt} [{elapsed}<{remaining}]')
    
    try:
        for dirpath, dirnames, filenames in os.walk(root_dir, topdown=False):
            if shutdown_requested:
                print(f"\n{Colors.YELLOW}🛑 Operation interrupted by user{Colors.RESET}")
                break
            
            # Update statistics for files in this directory
            stats['total_files'] += len(filenames)
            
            # Count file types and calculate total file size
            for filename in filenames:
                if shutdown_requested:
                    break
                    
                ext = os.path.splitext(filename)[1].lower()
                if ext:
                    stats[f'files_{ext[1:]}'] += 1
                else:
                    stats['files_no_extension'] += 1
                    
                # Calculate total file size
                try:
                    file_path = os.path.join(dirpath, filename)
                    file_size = os.path.getsize(file_path)
                    stats['total_size_bytes'] += file_size
                    
                    # Check if this is an image file that should be deleted
                    if ext in IMAGE_EXTENSIONS:
                        should_delete_img, reason = should_delete_image(file_path, filename)
                        if should_delete_img:
                            if pretend:
                                print(f"  {Colors.YELLOW}🖼️  Would delete image:{Colors.RESET} {filename} {Colors.CYAN}[Reason: {reason}]{Colors.RESET}")
                                print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {file_path} {Colors.CYAN}({format_size(file_size)}){Colors.RESET}")
                                stats['images_deleted'] += 1
                                stats['total_size_deleted_bytes'] += file_size
                            else:
                                print(f"  {Colors.RED}🖼️  Deleting image:{Colors.RESET} {filename} {Colors.CYAN}[Reason: {reason}]{Colors.RESET}")
                                print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {file_path} {Colors.CYAN}({format_size(file_size)}){Colors.RESET}")
                                try:
                                    os.remove(file_path)
                                    stats['images_deleted'] += 1
                                    stats['total_size_deleted_bytes'] += file_size
                                except OSError as e:
                                    print(f"    {Colors.RED}❌ Error deleting image: {e}{Colors.RESET}")
                        
                except (OSError, FileNotFoundError):
                    pass
            
            # Process ZIP files in this directory
            if filenames and not shutdown_requested:
                zip_files = [f for f in filenames if f.lower().endswith('.zip')]
                if zip_files:
                    print(f"\n{Colors.BLUE}📂 Processing ZIP files in: {dirpath}{Colors.RESET}")
                    unzip_files_in_directory(dirpath, pretend)
            
            # Process directories for potential deletion
            for dirname in dirnames:
                if shutdown_requested:
                    break
                    
                full_path = os.path.join(dirpath, dirname)
                processed_dirs += 1
                stats['total_directories'] += 1
                
                # Update progress bar
                if pbar:
                    pbar.update(1)
                
                # Check if directory should be deleted based on keywords/dates or '-1' suffix first
                if should_delete(dirname, delete_dash_one=delete_dash_one):
                    dir_size = get_directory_size(full_path)
                    highlighted_info = highlight_deletion_reason(dirname, delete_dash_one=delete_dash_one)
                    if pretend:
                        print(f"  {Colors.YELLOW}🗑️  Would delete:{Colors.RESET} {highlighted_info}")
                        print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {full_path} {Colors.CYAN}({format_size(dir_size)}){Colors.RESET}")
                        stats['keyword_directories_deleted'] += 1
                        deleted += 1
                        stats['total_size_deleted_bytes'] += dir_size
                    else:
                        print(f"  {Colors.RED}🗑️  Deleting:{Colors.RESET} {highlighted_info}")
                        print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {full_path} {Colors.CYAN}({format_size(dir_size)}){Colors.RESET}")
                        try:
                            shutil.rmtree(full_path)
                            stats['keyword_directories_deleted'] += 1
                            deleted += 1
                            stats['total_size_deleted_bytes'] += dir_size
                        except OSError as e:
                            print(f"    {Colors.RED}❌ Error deleting: {e}{Colors.RESET}")
                    continue
                
                # Check SFV integrity (only if not already marked for deletion and SFV checking is enabled)
                if check_sfv:
                    should_delete_sfv, sfv_reason, sfv_details, sfv_target_path = check_sfv_integrity(full_path)
                    if should_delete_sfv:
                        # Use the target path (might be parent directory if SFV is in 'extr')
                        dir_size = get_directory_size(sfv_target_path)
                        
                        # Get the display name for the target directory
                        target_dirname = os.path.basename(sfv_target_path)
                        
                        if pretend:
                            print(f"  {Colors.YELLOW}🗑️  Would delete (SFV failed):{Colors.RESET} {target_dirname}")
                            print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {sfv_target_path} {Colors.CYAN}({format_size(dir_size)}){Colors.RESET}")
                            print(f"    {Colors.CYAN}[Reason: {sfv_reason}]{Colors.RESET}")
                            if sfv_details:
                                print_sfv_details(sfv_details, dirname)
                            stats['sfv_failed_directories_deleted'] += 1
                            deleted += 1
                            stats['total_size_deleted_bytes'] += dir_size
                        else:
                            print(f"  {Colors.RED}🗑️  Deleting (SFV failed):{Colors.RESET} {target_dirname}")
                            print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {sfv_target_path} {Colors.CYAN}({format_size(dir_size)}){Colors.RESET}")
                            print(f"    {Colors.CYAN}[Reason: {sfv_reason}]{Colors.RESET}")
                            if sfv_details:
                                print_sfv_details(sfv_details, dirname)
                            try:
                                shutil.rmtree(sfv_target_path)
                                stats['sfv_failed_directories_deleted'] += 1
                                deleted += 1
                                stats['total_size_deleted_bytes'] += dir_size
                            except OSError as e:
                                print(f"    {Colors.RED}❌ Error deleting: {e}{Colors.RESET}")
                        continue
                
                # Check if directory is empty and delete it (only if not already matched above)
                elif is_directory_empty(full_path):
                    if pretend:
                        print(f"  {Colors.YELLOW}🗂️  Would delete empty directory:{Colors.RESET} {dirname}")
                        print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {full_path}")
                        stats['empty_directories_deleted'] += 1
                        deleted += 1
                    else:
                        print(f"  {Colors.GREEN}🗂️  Deleting empty directory:{Colors.RESET} {dirname}")
                        print(f"    {Colors.CYAN}📍 Path:{Colors.RESET} {full_path}")
                        try:
                            os.rmdir(full_path)
                            stats['empty_directories_deleted'] += 1
                            deleted += 1
                        except OSError as e:
                            print(f"    {Colors.RED}❌ Error deleting: {e}{Colors.RESET}")
                            continue
                
                # Small delay to allow for interruption on large operations
                if processed_dirs % 100 == 0:
                    time.sleep(0.001)
    
    finally:
        if pbar:
            pbar.close()
        
        if shutdown_requested:
            print(f"\n{Colors.YELLOW}⚠️  Operation was interrupted. Partial results displayed.{Colors.RESET}")
    
    return deleted

def print_statistics():
    """Print comprehensive statistics about the operation."""
    end_time = time.time()
    duration = end_time - start_time
    
    print(f"\n{Colors.BOLD}{Colors.BG_BLUE}                    OPERATION STATISTICS                    {Colors.RESET}")
    print(f"{Colors.BOLD}{'='*60}{Colors.RESET}")
    
    # Time and performance stats
    print(f"{Colors.CYAN}⏱️  Execution Time:{Colors.RESET} {duration:.2f} seconds")
    
    # Directory and file statistics
    print(f"\n{Colors.BOLD}{Colors.UNDERLINE}📁 Directory & File Overview:{Colors.RESET}")
    print(f"  {Colors.GREEN}📂 Total directories scanned:{Colors.RESET} {Colors.BOLD}{stats['total_directories']:,}{Colors.RESET}")
    print(f"  {Colors.GREEN}📄 Total files found:{Colors.RESET} {Colors.BOLD}{stats['total_files']:,}{Colors.RESET}")
    print(f"  {Colors.GREEN}💾 Total data size:{Colors.RESET} {Colors.BOLD}{format_size(stats['total_size_bytes'])}{Colors.RESET}")
    
    # File type breakdown
    file_types = []
    for key, value in stats.items():
        if key.startswith('files_') and value > 0:
            ext = key.replace('files_', '')
            if ext == 'no_extension':
                file_types.append(f"No extension: {value:,}")
            else:
                file_types.append(f".{ext}: {value:,}")
    
    if file_types:
        print(f"\n{Colors.BOLD}{Colors.UNDERLINE}📊 File Type Breakdown:{Colors.RESET}")
        for i, file_type in enumerate(sorted(file_types)[:10]):  # Show top 10
            print(f"  {Colors.BLUE}▪️{Colors.RESET} {file_type}")
        if len(file_types) > 10:
            print(f"  {Colors.YELLOW}... and {len(file_types) - 10} more file types{Colors.RESET}")
    
    # Processing statistics
    print(f"\n{Colors.BOLD}{Colors.UNDERLINE}🔧 Processing Results:{Colors.RESET}")
    print(f"  {Colors.MAGENTA}📦 ZIP files processed:{Colors.RESET} {Colors.BOLD}{stats['zip_files_processed']}{Colors.RESET}")
    print(f"  {Colors.YELLOW}🗂️  Empty directories removed:{Colors.RESET} {Colors.BOLD}{stats['empty_directories_deleted']}{Colors.RESET}")
    print(f"  {Colors.RED}🗑️  Keyword/date directories removed:{Colors.RESET} {Colors.BOLD}{stats['keyword_directories_deleted']}{Colors.RESET}")
    print(f"  {Colors.RED}📋 SFV failed directories removed:{Colors.RESET} {Colors.BOLD}{stats.get('sfv_failed_directories_deleted', 0)}{Colors.RESET}")
    print(f"  {Colors.CYAN}🖼️  Images deleted:{Colors.RESET} {Colors.BOLD}{stats['images_deleted']}{Colors.RESET}")
    
    total_deleted = stats['empty_directories_deleted'] + stats['keyword_directories_deleted'] + stats.get('sfv_failed_directories_deleted', 0)
    print(f"\n{Colors.BOLD}{Colors.BG_GREEN} TOTAL DIRECTORIES DELETED: {total_deleted} {Colors.RESET}")
    
    if stats.get('images_deleted', 0) > 0:
        print(f"{Colors.BOLD}{Colors.BG_BLUE} TOTAL IMAGES DELETED: {stats['images_deleted']} {Colors.RESET}")
    
    if stats.get('total_size_deleted_bytes', 0) > 0:
        print(f"{Colors.BOLD}{Colors.BG_RED} TOTAL DATA FREED: {format_size(stats['total_size_deleted_bytes'])} {Colors.RESET}")
    
    # Efficiency metrics
    if stats['total_directories'] > 0:
        deletion_rate = (total_deleted / stats['total_directories']) * 100
        print(f"\n{Colors.CYAN}📈 Deletion Rate:{Colors.RESET} {deletion_rate:.1f}% of directories processed")
    
    print(f"{Colors.BOLD}{'='*60}{Colors.RESET}")
    
    if total_deleted > 0:
        print(f"{Colors.GREEN}✅ Cleanup completed successfully!{Colors.RESET}")
    else:
        print(f"{Colors.BLUE}ℹ️  No directories matched deletion criteria.{Colors.RESET}")

# Signal handler for graceful shutdown
def signal_handler(signum, frame):
    """Handle interrupt signals gracefully."""
    global shutdown_requested
    print(f"\n{Colors.YELLOW}🛑 Interrupt received. Finishing current operation...{Colors.RESET}")
    shutdown_requested = True
    
def setup_signal_handlers():
    """Setup signal handlers for graceful shutdown."""
    signal.signal(signal.SIGINT, signal_handler)
    signal.signal(signal.SIGTERM, signal_handler)

def get_image_dimensions(file_path):
    """
    Get image dimensions using PIL.
    
    Args:
        file_path: Path to image file
    
    Returns:
        Tuple of (width, height) or None if unable to read
    """
    if not PIL_AVAILABLE:
        return None
    
    try:
        with Image.open(file_path) as img:
            return img.size
    except (OSError, IOError, Image.UnidentifiedImageError):
        return None

def should_delete_image(file_path, filename):
    """
    Check if an image file should be deleted based on size or "proof" in name.
    
    Args:
        file_path: Full path to the file
        filename: Just the filename
    
    Returns:
        Tuple of (should_delete: bool, reason: str)
    """
    # Check if filename contains "proof"
    if PROOF_PATTERN.search(filename):
        return True, "contains 'proof'"
    
    # Check image dimensions if PIL is available
    if PIL_AVAILABLE:
        dimensions = get_image_dimensions(file_path)
        if dimensions:
            width, height = dimensions
            if width < 300 or height < 300:
                return True, f"small resolution ({width}x{height})"
    
    return False, ""

def calculate_crc32(file_path):
    """
    Calculate CRC32 checksum for a file.
    
    Args:
        file_path: Path to the file
    
    Returns:
        CRC32 checksum as an 8-character uppercase hex string, or None if error
    """
    try:
        crc = 0
        with open(file_path, 'rb') as f:
            while True:
                chunk = f.read(65536)  # 64KB chunks
                if not chunk:
                    break
                crc = zlib.crc32(chunk, crc)
        
        # Convert to unsigned 32-bit value and format as 8-char hex
        return f"{crc & 0xffffffff:08X}"
    except (OSError, IOError) as e:
        return None

def parse_sfv_file(sfv_path):
    """
    Parse an SFV file and return a dictionary of filename -> expected_crc32.
    Handles various SFV formats and encodings robustly.
    
    Args:
        sfv_path: Path to the SFV file
    
    Returns:
        Dictionary mapping relative filenames to expected CRC32 checksums
    """
    file_checksums = {}
    
    # Try different encodings
    encodings = ['utf-8', 'latin1', 'cp1252', 'ascii']
    
    for encoding in encodings:
        try:
            with open(sfv_path, 'r', encoding=encoding) as f:
                lines = f.readlines()
            break
        except (UnicodeDecodeError, OSError):
            continue
    else:
        print(f"  {Colors.RED}❌ Could not read SFV file with any encoding: {sfv_path}{Colors.RESET}")
        return file_checksums
    
    for line_num, line in enumerate(lines, 1):
        line = line.strip()
        
        # Skip empty lines and comments
        if not line or line.startswith(';') or line.startswith('#'):
            continue
        
        # Handle various SFV formats
        # Format 1: filename.ext CRC32HASH
        # Format 2: filename.ext CRC32HASH size
        # Format 3: "filename with spaces.ext" CRC32HASH
        
        # Try quoted filename first
        if line.startswith('"'):
            quote_end = line.find('"', 1)
            if quote_end != -1:
                filename = line[1:quote_end]
                remainder = line[quote_end + 1:].strip()
            else:
                continue  # Malformed quoted line
        else:
            # Split on whitespace
            parts = line.split()
            if len(parts) < 2:
                continue  # Not enough parts
            
            # Last part should be CRC32 (8 hex chars)
            potential_crc = parts[-1].upper()
            if len(potential_crc) == 8 and all(c in '0123456789ABCDEF' for c in potential_crc):
                # CRC32 is last part, filename is everything before it
                filename = ' '.join(parts[:-1])
                remainder = potential_crc
            else:
                continue  # No valid CRC32 found
        
        # Extract CRC32 from remainder
        if remainder:
            crc_parts = remainder.split()
            if crc_parts:
                crc32 = crc_parts[0].upper()
                # Validate CRC32 format
                if len(crc32) == 8 and all(c in '0123456789ABCDEF' for c in crc32):
                    file_checksums[filename] = crc32
                else:
                    print(f"  {Colors.YELLOW}⚠️  Invalid CRC32 format at line {line_num}: {crc32}{Colors.RESET}")
    
    return file_checksums

def verify_sfv_file(sfv_path, search_dirs=None):
    """
    Verify all files listed in an SFV file.
    Handles cases where SFV is in 'extr' subdirectory but files are in parent directory.
    
    Args:
        sfv_path: Path to the SFV file
        search_dirs: Optional list of directories to search for files
    
    Returns:
        Tuple of (all_passed: bool, results: dict)
        results dict contains: {'filename': {'expected': 'CRC32', 'actual': 'CRC32', 'status': 'PASS/FAIL/MISSING'}}
    """
    global shutdown_requested
    
    sfv_dir = os.path.dirname(sfv_path)
    file_checksums = parse_sfv_file(sfv_path)
    
    if not file_checksums:
        return False, {}
    
    results = {}
    all_passed = True
    
    # Use provided search_dirs or determine them automatically
    if search_dirs is None:
        # Check if SFV is in an 'extr' directory - if so, also check parent directory for files
        sfv_dir_name = os.path.basename(sfv_dir).lower()
        search_dirs = [sfv_dir]
        
        if sfv_dir_name == 'extr':
            parent_dir = os.path.dirname(sfv_dir)
            search_dirs.append(parent_dir)
            print(f"    {Colors.BLUE}📁 SFV in 'extr' directory - will also search parent directory{Colors.RESET}")
    
    for filename, expected_crc in file_checksums.items():
        if shutdown_requested:
            break
        
        # Skip files with "proof" in the filename - these are optional
        if PROOF_PATTERN.search(filename):
            print(f"    {Colors.BLUE}📸 Skipping proof file:{Colors.RESET} {filename}")
            continue
        
        # Try to find the file in search directories (case-insensitive)
        file_found = False
        file_path = None
        actual_filename = None
        
        for search_dir in search_dirs:
            # First try exact match
            potential_path = os.path.join(search_dir, filename)
            if os.path.exists(potential_path):
                file_path = potential_path
                actual_filename = filename
                file_found = True
                break
            
            # If exact match fails, try case-insensitive search and encoding variants
            try:
                dir_files = os.listdir(search_dir)
                for dir_file in dir_files:
                    # Case-insensitive exact match
                    if dir_file.lower() == filename.lower():
                        file_path = os.path.join(search_dir, dir_file)
                        actual_filename = dir_file
                        file_found = True
                        break
                    # Check for files with encoding corruption suffix
                    elif '(invalid encoding)' in dir_file.lower():
                        # Extract the base filename without the encoding suffix
                        base_name = dir_file
                        
                        # Remove various encoding suffixes
                        for suffix in [' (invalid encoding).mp3', '(invalid encoding).mp3', ' (invalid encoding)', '(invalid encoding)']:
                            if base_name.lower().endswith(suffix.lower()):
                                base_name = base_name[:-len(suffix)]
                                break
                        
                        # For encoding-corrupted files, we need to be more flexible in matching
                        # Remove file extension from both for comparison
                        target_base = os.path.splitext(filename)[0]
                        actual_base = os.path.splitext(base_name)[0]
                        
                        # Try different approaches to match corrupted encoding
                        # 1. Direct comparison (ignoring case)
                        if actual_base.lower() == target_base.lower():
                            file_path = os.path.join(search_dir, dir_file)
                            actual_filename = dir_file
                            file_found = True
                            break
                        
                        # 2. Try to match by normalizing both strings and removing special characters
                        import unicodedata
                        try:
                            # Normalize and remove special characters from target
                            normalized_target = unicodedata.normalize('NFKD', target_base).encode('ascii', 'ignore').decode('ascii')
                            
                            # For actual filename, replace common corruption characters and normalize
                            cleaned_actual = actual_base.replace('�', '').replace('?', '')
                            normalized_actual = unicodedata.normalize('NFKD', cleaned_actual).encode('ascii', 'ignore').decode('ascii')
                            
                            # Also try removing the corrupted character entirely and seeing if strings match
                            if len(normalized_actual) > 0 and len(normalized_target) > 0:
                                # Compare similarity - if they're very close, consider it a match
                                if abs(len(normalized_target) - len(normalized_actual)) <= 2:
                                    # Simple substring match approach
                                    shorter = normalized_actual if len(normalized_actual) < len(normalized_target) else normalized_target
                                    longer = normalized_target if len(normalized_actual) < len(normalized_target) else normalized_actual
                                    
                                    if shorter.lower() in longer.lower() and len(shorter) > 5:  # Reasonable minimum length
                                        file_path = os.path.join(search_dir, dir_file)
                                        actual_filename = dir_file
                                        file_found = True
                                        break
                        except:
                            pass
                        
                        # 3. Try character-by-character comparison, ignoring corruption characters
                        try:
                            target_chars = [c for c in target_base.lower()]
                            actual_chars = [c for c in actual_base.lower() if ord(c) != 65533]  # Skip replacement character
                            
                            # More lenient heuristic: if 75% of characters match in order, consider it a match
                            matches = 0
                            min_len = min(len(target_chars), len(actual_chars))
                            for i in range(min_len):
                                if target_chars[i] == actual_chars[i]:
                                    matches += 1
                            
                            if min_len > 0 and matches / min_len >= 0.75:
                                file_path = os.path.join(search_dir, dir_file)
                                actual_filename = dir_file
                                file_found = True
                                break
                        except:
                            pass
                if file_found:
                    break
            except OSError:
                continue
        
        if not file_found:
            results[filename] = {
                'expected': expected_crc,
                'actual': None,
                'status': 'MISSING',
                'actual_filename': None,
                'rename_needed': False
            }
            all_passed = False
            continue
        
        actual_crc = calculate_crc32(file_path)
        
        if actual_crc is None:
            results[filename] = {
                'expected': expected_crc,
                'actual': None,
                'status': 'ERROR',
                'actual_filename': actual_filename,
                'rename_needed': False
            }
            all_passed = False
            continue
        
        status = 'PASS' if actual_crc == expected_crc else 'FAIL'
        
        # Check if file needs to be renamed (has encoding issues but passes validation)
        rename_needed = False
        if (status == 'PASS' and actual_filename != filename and 
            actual_filename and '(invalid encoding)' in actual_filename):
            rename_needed = True
        
        results[filename] = {
            'expected': expected_crc,
            'actual': actual_crc,
            'status': status,
            'actual_filename': actual_filename,
            'rename_needed': rename_needed,
            'file_path': file_path if rename_needed else None
        }
        
        if status != 'PASS':
            all_passed = False
    
    return all_passed, results

def rename_encoding_files(sfv_results, search_dirs, pretend=True):
    """
    Rename files that have encoding issues but pass SFV validation.
    
    Args:
        sfv_results: Results from verify_sfv_file
        search_dirs: Directories where files are located
        pretend: Whether to actually rename or just show what would be renamed
    
    Returns:
        Number of files renamed
    """
    renamed_count = 0
    
    for filename, result in sfv_results.items():
        if result.get('rename_needed', False) and result.get('file_path'):
            old_path = result['file_path']
            actual_filename = result['actual_filename']
            
            # Calculate new path in the same directory
            file_dir = os.path.dirname(old_path)
            new_path = os.path.join(file_dir, filename)
            
            # Make sure the target filename doesn't already exist
            if os.path.exists(new_path):
                if pretend:
                    print(f"        {Colors.YELLOW}⚠️  Would skip rename:{Colors.RESET} target exists")
                    print(f"          {actual_filename} → {filename}")
                continue
            
            try:
                if pretend:
                    print(f"        {Colors.GREEN}📝 Would rename:{Colors.RESET} {actual_filename}")
                    print(f"          → {filename}")
                else:
                    print(f"        {Colors.GREEN}📝 Renaming:{Colors.RESET} {actual_filename}")
                    print(f"          → {filename}")
                    os.rename(old_path, new_path)
                
                renamed_count += 1
                
            except OSError as e:
                print(f"        {Colors.RED}❌ Rename failed:{Colors.RESET} {e}")
    
    return renamed_count
def check_sfv_integrity(directory_path):
    """
    Check if a directory contains SFV files and verify their integrity.
    Handles 'extr' subdirectories where SFV might reference files in parent directory.
    
    Args:
        directory_path: Path to directory to check
    
    Returns:
        Tuple of (should_delete: bool, reason: str, details: dict, target_path: str)
        target_path: The actual directory that should be deleted (might be parent if SFV is in 'extr')
    """
    global shutdown_requested
    
    if shutdown_requested:
        return False, "", {}, directory_path
    
    try:
        files = os.listdir(directory_path)
    except OSError:
        return False, "", {}, directory_path
    
    sfv_files = [f for f in files if f.lower().endswith('.sfv')]
    
    if not sfv_files:
        return False, "", {}, directory_path  # No SFV files to verify
    
    failed_sfv_files = []
    all_results = {}
    
    # Check if this is an 'extr' directory
    dir_name = os.path.basename(directory_path).lower()
    is_extr_dir = dir_name == 'extr'
    
    for sfv_file in sfv_files:
        if shutdown_requested:
            break
            
        sfv_path = os.path.join(directory_path, sfv_file)
        
        # Search in SFV file directory first, then in parent directory (for "extr" case)
        search_dirs = [directory_path]
        
        # If this is an extr directory, also search parent directory
        if is_extr_dir:
            parent_dir = os.path.dirname(directory_path)
            if os.path.exists(parent_dir):
                search_dirs.append(parent_dir)
        
        all_passed, results = verify_sfv_file(sfv_path, search_dirs)
        
        # Handle file renaming for encoding issues if SFV passed
        if all_passed:
            rename_encoding_files(results, search_dirs, pretend=False)
        
        all_results[sfv_file] = {
            'passed': all_passed,
            'results': results
        }
        
        if not all_passed:
            failed_sfv_files.append(sfv_file)
    
    if failed_sfv_files:
        # If SFV is in 'extr' directory and failed, suggest deleting parent directory
        target_path = os.path.dirname(directory_path) if is_extr_dir else directory_path
        reason = f"SFV verification failed for: {', '.join(failed_sfv_files)}"
        if is_extr_dir:
            reason += " (in 'extr' subdirectory)"
        return True, reason, all_results, target_path
    
    return False, "", all_results, directory_path

def print_sfv_details(sfv_results, directory_name):
    """
    Print detailed SFV verification results.
    
    Args:
        sfv_results: Results dictionary from check_sfv_integrity
        directory_name: Name of the directory being checked
    """
    if not sfv_results:
        return
    
    print(f"    {Colors.CYAN}📋 SFV Verification Details for {directory_name}:{Colors.RESET}")
    
    for sfv_file, data in sfv_results.items():
        passed = data['passed']
        results = data['results']
        
        status_color = Colors.GREEN if passed else Colors.RED
        status_text = "✅ PASSED" if passed else "❌ FAILED"
        
        print(f"      {status_color}{status_text}:{Colors.RESET} {sfv_file}")
        
        if not passed:
            # Show details of failed files
            for filename, file_result in results.items():
                if file_result['status'] != 'PASS':
                    status = file_result['status']
                    expected = file_result['expected']
                    actual = file_result['actual']
                    actual_filename = file_result.get('actual_filename')
                    
                    if status == 'MISSING':
                        print(f"        {Colors.YELLOW}🔍 MISSING:{Colors.RESET} {filename}")
                    elif status == 'FAIL':
                        print(f"        {Colors.RED}💥 CRC MISMATCH:{Colors.RESET} {filename}")
                        if actual_filename and actual_filename != filename:
                            print(f"          Found as: {actual_filename} (case mismatch)")
                        print(f"          Expected: {expected}")
                        print(f"          Actual:   {actual}")
                    elif status == 'ERROR':
                        error_name = actual_filename if actual_filename else filename
                        print(f"        {Colors.RED}❌ READ ERROR:{Colors.RESET} {error_name}")
                        if actual_filename and actual_filename != filename:
                            print(f"          Found as: {actual_filename} (case mismatch)")

if __name__ == "__main__":
    # Setup signal handlers for graceful shutdown
    setup_signal_handlers()
    parser = argparse.ArgumentParser(description='Clean up directories based on keywords, dates, SFV integrity, and duplicate "-1" suffix')
    parser.add_argument('--pretend', type=str, choices=['true', 'false'], default='true',
                        help='Pretend mode: true (default, safe mode - only show what would be deleted) or false (actually delete)')
    parser.add_argument('--root-dir', default='.',
                        help='Root directory to search from (default: current directory)')
    parser.add_argument('--check-sfv', type=str, choices=['true', 'false'], default='true',
                        help='Enable SFV integrity checking: true (default) or false (skip SFV verification)')
    parser.add_argument('--delete-dash-one', type=str, choices=['true', 'false'], default='true',
                        help='Delete directories ending with "-1" (likely duplicates): true (default) or false')
    args = parser.parse_args()
    pretend_mode = args.pretend.lower() == 'true'
    check_sfv_enabled = args.check_sfv.lower() == 'true'
    delete_dash_one_enabled = args.delete_dash_one.lower() == 'true'
    print(f"{Colors.BOLD}{Colors.BG_BLUE}                  NEWS GROUP CLEANUP TOOL                  {Colors.RESET}")
    print(f"{Colors.BOLD}{'='*60}{Colors.RESET}")
    if pretend_mode:
        print(f"{Colors.YELLOW}🔍 PRETEND MODE:{Colors.RESET} Showing what would be deleted (use {Colors.BOLD}--pretend false{Colors.RESET} to actually delete)")
    else:
        print(f"{Colors.RED}⚠️  LIVE MODE:{Colors.RESET} {Colors.BOLD}Actually deleting directories{Colors.RESET}")
        print(f"{Colors.RED}⚠️  WARNING:{Colors.RESET} This will permanently delete directories!")
    print(f"{Colors.CYAN}📁 Target Directory:{Colors.RESET} {Colors.BOLD}{os.path.abspath(args.root_dir)}{Colors.RESET}")
    print(f"{Colors.MAGENTA}🎯 Keywords:{Colors.RESET} {', '.join(KEYWORDS)}")
    if check_sfv_enabled:
        print(f"{Colors.BLUE}📋 SFV Checking:{Colors.RESET} {Colors.GREEN}Enabled{Colors.RESET} (use {Colors.BOLD}--check-sfv false{Colors.RESET} to disable)")
    else:
        print(f"{Colors.BLUE}📋 SFV Checking:{Colors.RESET} {Colors.YELLOW}Disabled{Colors.RESET}")
    print(f"{Colors.YELLOW}🗂️  Delete '-1' Duplicates:{Colors.RESET} {'Enabled' if delete_dash_one_enabled else 'Disabled'} (use --delete-dash-one false to disable)")
    print(f"{Colors.BOLD}{'-' * 60}{Colors.RESET}")
    ROOT_DIR = args.root_dir
    deleted_count = delete_matching_dirs(ROOT_DIR, pretend_mode, check_sfv_enabled, delete_dash_one=delete_dash_one_enabled)
    print_statistics()